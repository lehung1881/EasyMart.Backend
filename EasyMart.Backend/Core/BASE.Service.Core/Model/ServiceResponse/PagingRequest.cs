using BASE.Service.Core.Enum;
using BASE.Service.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Model
{
    /// <summary>
    /// Model đại diện cho yêu cầu phân trang, lọc và sắp xếp dữ liệu.
    /// </summary>
    public class PagingRequest
    {
        /// <summary>
        /// Danh sách điều kiện sắp xếp dữ liệu.
        /// </summary>
        public List<SortCondition> Sort { get; set; }

        /// <summary>
        /// Điều kiện lọc dữ liệu.
        /// </summary>
        public FilterCondition Filter { get; set; }

        /// <summary>
        /// Danh sách các cột cần lấy dữ liệu (phân tách bởi dấu phẩy hoặc định dạng tùy chỉnh).
        /// </summary>
        public string Columns { get; set; }

        /// <summary>
        /// Chỉ số trang hiện tại (bắt đầu từ 1).
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Số lượng bản ghi trên mỗi trang.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Tên view hoặc tên bảng dữ liệu cần truy vấn.
        /// </summary>
        public string ViewOrTableName { get; set; }

        /// <summary>
        /// Giá trị đang selected trong Combobox
        /// </summary>
        public SelectedValue SelectedValue { get; set; }
    }

    /// <summary>
    /// Model đại diện cho một điều kiện sắp xếp.
    /// </summary>
    public class SortCondition
    {
        /// <summary>
        /// Tên thuộc tính (cột) cần sắp xếp.
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// Sắp xếp giảm dần nếu true, tăng dần nếu false.
        /// </summary>
        public bool Desc { get; set; }

        /// <summary>
        /// Kiểu dữ liệu của thuộc tính.
        /// </summary>
        public DataType DataType { get; set; }

        /// <summary>
        /// Toán hạng xác định ngữ cảnh sắp xếp.
        /// </summary>
        public int Operand { get; set; }
    }

    /// <summary>
    /// Đại diện cho một node trong cây bộ lọc (Filter Tree).
    /// Node này có thể là một điều kiện đơn lẻ (Condition) hoặc một nhóm các điều kiện lồng nhau (Group).
    /// </summary>
    public class FilterCondition
    {
        /// <summary>
        /// Xác định loại của node hiện tại (Là điều kiện đơn hay là một nhóm điều kiện).
        /// </summary>
        /// <value>Mặc định là <see cref="FilterNodeType.Condition"/>.</value>
        public FilterNodeType NodeType { get; set; } = FilterNodeType.Condition;

        #region Các thuộc tính dành riêng cho NodeType = Condition

        /// <summary>
        /// Tên thuộc tính hoặc trường dữ liệu cần áp dụng bộ lọc (ví dụ: "Age", "CreatedDate").
        /// </summary>
        /// <remarks>Chỉ có giá trị và được sử dụng khi <see cref="NodeType"/> là <see cref="FilterNodeType.Condition"/>.</remarks>
        public string? Property { get; set; }

        /// <summary>
        /// Giá trị dùng để so sánh trong điều kiện lọc.
        /// </summary>
        /// <remarks>Chỉ có giá trị và được sử dụng khi <see cref="NodeType"/> là <see cref="FilterNodeType.Condition"/>.</remarks>
        public object? Value { get; set; }

        /// <summary>
        /// Toán tử so sánh được áp dụng cho điều kiện (ví dụ: Equal, GreaterThan, Contains).
        /// </summary>
        /// <remarks>Chỉ có giá trị và được sử dụng khi <see cref="NodeType"/> là <see cref="FilterNodeType.Condition"/>.</remarks>
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// Kiểu dữ liệu của thuộc tính cần lọc (ví dụ: String, Number, DateTime, Boolean).
        /// </summary>
        /// <remarks>Chỉ có giá trị và được sử dụng khi <see cref="NodeType"/> là <see cref="FilterNodeType.Condition"/>.</remarks>
        public DataType DataType { get; set; }

        #endregion

        #region Các thuộc tính dành riêng cho NodeType = Group

        /// <summary>
        /// Toán tử logic dùng để liên kết các điều kiện con bên trong nhóm (AND hoặc OR).
        /// </summary>
        /// <value>Mặc định là <see cref="LogicalOperator.And"/>.</value>
        /// <remarks>Chỉ có giá trị và được sử dụng khi <see cref="NodeType"/> là <see cref="FilterNodeType.Group"/>.</remarks>
        public LogicalOperator LogicalOperator { get; set; } = LogicalOperator.And;

        /// <summary>
        /// Danh sách các điều kiện con hoặc nhóm con thuộc về nhóm hiện tại.
        /// </summary>
        /// <remarks>Chỉ có giá trị và được sử dụng khi <see cref="NodeType"/> là <see cref="FilterNodeType.Group"/>.</remarks>
        public List<FilterCondition> Children { get; set; } = new();

        #endregion
    }

    /// <summary>
    /// Selected value dùng cho combobox
    /// </summary>
    public class SelectedValue
    {
        /// <summary>
        /// Tên thuộc tính (cột) cần lọc.
        /// </summary>
        public string Property { get; set; }

        /// <summary>
        /// Giá trị dùng để so sánh khi lọc (có thể là string, số, DateTime,...).
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Kiểu dữ liệu của thuộc tính.
        /// </summary>
        public DataType DataType { get; set; }
    }

    /// <summary>
    /// Kết quả build SQL từ PagingRequest.
    /// </summary>
    public class PagingSQLBuilder
    {
        /// <summary>
        /// Câu SQL đã build (bao gồm WHERE, ORDER BY, LIMIT/OFFSET).
        /// </summary>
        public string PagingQuery { get; set; }

        /// <summary>
        /// Câu SQL đếm tổng số bản ghi (dùng cho phân trang).
        /// </summary>
        public string PagingQueryCount { get; set; }

        /// <summary>
        /// Dictionary chứa các tham số truyền vào SQL.
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }
    }
}
