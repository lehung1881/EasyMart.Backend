using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Utils
{
    /// <summary>
    /// Build mệnh đề WHERE từ cây FilterCondition.
    /// </summary>
    public class FilterQueryBuilder
    {
        private int _paramIndex = 0;

        /// <summary>
        /// Entry point: nhận root FilterCondition, trả về WHERE clause và parameters.
        /// </summary>
        public (string WhereClause, Dictionary<string, object> Parameters) Build(FilterCondition root)
        {
            var parameters = new Dictionary<string, object>();

            if (root == null)
                return (string.Empty, parameters);

            var clause = BuildNode(root, parameters);

            if (clause.StartsWith("(") && clause.EndsWith(")"))
                clause = clause[1..^1];

            return (clause, parameters);
        }

        /// <summary>
        /// Đệ quy xử lý FilterCondition — dispatch sang BuildGroup hoặc BuildLeaf.
        /// </summary>
        private string BuildNode(FilterCondition node, Dictionary<string, object> parameters)
            => node.NodeType switch
            {
                FilterNodeType.Group => BuildGroup(node, parameters),
                FilterNodeType.Condition => BuildLeaf(node, parameters),
                _ => string.Empty
            };

        /// <summary>
        /// Build Group node: nối các Children bằng LogicalOperator, bọc ngoặc khi có >1 con.
        /// </summary>
        private string BuildGroup(FilterCondition group, Dictionary<string, object> parameters)
        {
            if (group.Children == null || group.Children.Count == 0)
                return string.Empty;

            var parts = group.Children
                .Select(child => BuildNode(child, parameters))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList();

            if (parts.Count == 0) return string.Empty;
            if (parts.Count == 1) return parts[0];

            var joiner = group.LogicalOperator == LogicalOperator.Or ? " OR " : " AND ";
            return $"({string.Join(joiner, parts)})";
        }

        /// <summary>
        /// Build Condition node thành SQL condition string, ghi param vào dictionary.
        /// </summary>
        private string BuildLeaf(FilterCondition condition, Dictionary<string, object> parameters)
        {
            var property = $"`{condition.Property}`";
            var paramName = $"@p{_paramIndex++}";
            var converted = ConvertUtil.ConvertValueByDataType(condition.DataType, condition.Value) ?? condition.Value;

            return condition.Operator switch
            {
                FilterOperator.Equal => AddParam(parameters, paramName, converted, $"{property} = {paramName}"),
                FilterOperator.NotEqual => AddParam(parameters, paramName, converted, $"{property} <> {paramName}"),
                FilterOperator.LessThan => AddParam(parameters, paramName, converted, $"{property} < {paramName}"),
                FilterOperator.LessThanOrEqual => AddParam(parameters, paramName, converted, $"{property} <= {paramName}"),
                FilterOperator.GreaterThan => AddParam(parameters, paramName, converted, $"{property} > {paramName}"),
                FilterOperator.GreaterThanOrEqual => AddParam(parameters, paramName, converted, $"{property} >= {paramName}"),
                FilterOperator.Contains => AddParam(parameters, paramName, $"%{converted}%", $"{property} LIKE {paramName}"),
                FilterOperator.NotContains => AddParam(parameters, paramName, $"%{converted}%", $"{property} NOT LIKE {paramName}"),
                FilterOperator.StartsWith => AddParam(parameters, paramName, $"{converted}%", $"{property} LIKE {paramName}"),
                FilterOperator.EndsWith => AddParam(parameters, paramName, $"%{converted}", $"{property} LIKE {paramName}"),
                FilterOperator.IsNull => $"({property} IS NULL)",
                FilterOperator.IsNullOrEmpty => $"({property} IS NULL OR {property} = '')",
                FilterOperator.IsNotNull => $"({property} IS NOT NULL)",
                FilterOperator.IsNotNullOrEmpty => $"({property} IS NOT NULL AND {property} <> '')",
                FilterOperator.In => BuildInClause(condition, property, parameters, negate: false),
                FilterOperator.NotIn => BuildInClause(condition, property, parameters, negate: true),
                FilterOperator.Between => BuildBetweenClause(condition, property, parameters),
                _ => string.Empty
            };
        }

        /// <summary>
        /// Helper: ghi param vào dictionary và trả về clause string.
        /// </summary>
        private static string AddParam(Dictionary<string, object> parameters, string name, object? value, string clause)
        {
            parameters[name] = value!;
            return clause;
        }

        /// <summary>
        /// Build IN / NOT IN — mỗi phần tử trong list là một param riêng biệt.
        /// </summary>
        private string BuildInClause(FilterCondition condition, string property,
            Dictionary<string, object> parameters, bool negate)
        {
            var keyword = negate ? "NOT IN" : "IN";

            if (condition.Value is IEnumerable<object> values)
            {
                var paramNames = values.Select(val =>
                {
                    var pName = $"@p{_paramIndex++}";
                    parameters[pName] = ConvertUtil.ConvertValueByDataType(condition.DataType, val) ?? val;
                    return pName;
                }).ToList();

                return $"{property} {keyword} ({string.Join(", ", paramNames)})";
            }

            var single = $"@p{_paramIndex++}";
            parameters[single] = condition.Value!;
            return $"{property} {keyword} ({single})";
        }

        /// <summary>
        /// Build BETWEEN — Value phải là IList có đúng 2 phần tử [from, to].
        /// </summary>
        private string BuildBetweenClause(FilterCondition condition, string property,
            Dictionary<string, object> parameters)
        {
            if (condition.Value is not IList<object> range || range.Count != 2)
                return string.Empty;

            var fromParam = $"@p{_paramIndex++}";
            var toParam = $"@p{_paramIndex++}";
            parameters[fromParam] = ConvertUtil.ConvertValueByDataType(condition.DataType, range[0]) ?? range[0];
            parameters[toParam] = ConvertUtil.ConvertValueByDataType(condition.DataType, range[1]) ?? range[1];

            return $"{property} BETWEEN {fromParam} AND {toParam}";
        }
    }

    /// <summary>
    /// Xác định loại của một node trong cây bộ lọc (Filter Tree).
    /// </summary>
    public enum FilterNodeType
    {
        /// <summary>
        /// Node là một điều kiện lọc đơn lẻ (bao gồm Thuộc tính, Toán tử và Giá trị).
        /// </summary>
        Condition,

        /// <summary>
        /// Node là một nhóm chứa danh sách các điều kiện con hoặc các nhóm con khác lồng nhau.
        /// </summary>
        Group
    }

    /// <summary>
    /// Xác định toán tử logic dùng để liên kết các điều kiện hoặc nhóm điều kiện với nhau.
    /// </summary>
    public enum LogicalOperator
    {
        /// <summary>
        /// Toán tử logic AND (Tất cả các điều kiện trong nhóm đều phải thỏa mãn).
        /// </summary>
        And,

        /// <summary>
        /// Toán tử logic OR (Chỉ cần ít nhất một điều kiện trong nhóm thỏa mãn).
        /// </summary>
        Or
    }
}
