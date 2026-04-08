using BASE.Service.Core.Enum;
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
        /// Danh sách điều kiện lọc dữ liệu.
        /// </summary>
        public List<FilterCondition> Filter { get; set; }

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
    /// Model đại diện cho một điều kiện lọc dữ liệu.
    /// </summary>
    public class FilterCondition
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
        /// Toán tử so sánh.
        /// </summary>
        public FilterOperator Operator { get; set; }

        /// <summary>
        /// Toán hạng xác định ngữ cảnh lọc.
        /// </summary>
        public int Operand { get; set; }

        /// <summary>
        /// Kiểu dữ liệu của thuộc tính.
        /// </summary>
        public DataType DataType { get; set; }
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
