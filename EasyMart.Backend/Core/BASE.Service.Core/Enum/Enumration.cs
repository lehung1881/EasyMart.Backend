using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Enum
{
    public enum ServiceResponseCode : int
    {
        /// <summary>
        /// Thành công.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Dữ liệu đầu vào không hợp lệ.
        /// </summary>
        InvalidData = 1,

        /// <summary>
        /// Không tìm thấy dữ liệu.
        /// </summary>
        NotFound = 2,

        /// <summary>
        /// Lỗi ngoại lệ không xác định.
        /// </summary>
        Exception = 3,

        /// <summary>
        /// Mã đã tồn tại trong hệ thống.
        /// </summary>
        ExistsCode = 4,

        /// <summary>
        /// Dữ liệu bị trùng lặp.
        /// </summary>
        Duplicate = 5,

        /// <summary>
        /// Token đã hết hạn hoặc không còn hiệu lực.
        /// </summary>
        TokenExpired = 6
    }

    /// <summary>
    /// Danh sách các trạng thái của bản ghi trong hệ thống
    /// </summary>
    public enum RecordStatus
    {
        /// <summary>
        /// Đang sử dụng
        /// </summary>
        Active = 1,

        /// <summary>
        /// Ngừng sử dụng
        /// </summary>
        Inactive = 2
    }


    public enum ModelState : int
    {
        None = 0,

        Insert = 1,

        Update = 2,

        Delete = 3,

    }

    /// <summary>
    /// Enum định nghĩa các toán tử so sánh dùng trong điều kiện lọc.
    /// </summary>
    public enum FilterOperator : int
    {
        /// <summary>
        /// Bằng (=).
        /// </summary>
        Equal = 1,

        /// <summary>
        /// Không bằng (!=).
        /// </summary>
        NotEqual = 2,

        /// <summary>
        /// Chứa chuỗi con (LIKE '%value%').
        /// </summary>
        Contains = 3,

        /// <summary>
        /// Không chứa chuỗi con.
        /// </summary>
        NotContains = 4,

        /// <summary>
        /// Bắt đầu bằng (LIKE 'value%').
        /// </summary>
        StartsWith = 5,

        /// <summary>
        /// Kết thúc bằng (LIKE '%value').
        /// </summary>
        EndsWith = 6,

        /// <summary>
        /// Là null hoặc rỗng.
        /// </summary>
        IsNullOrEmpty = 7,

        /// <summary>
        /// Không null và không rỗng.
        /// </summary>
        IsNotNullOrEmpty = 8,

        /// <summary>
        /// Nhỏ hơn (<).
        /// </summary>
        LessThan = 9,

        /// <summary>
        /// Nhỏ hơn hoặc bằng (<=).
        /// </summary>
        LessThanOrEqual = 10,

        /// <summary>
        /// Lớn hơn (>).
        /// </summary>
        GreaterThan = 11,

        /// <summary>
        /// Lớn hơn hoặc bằng (>=).
        /// </summary>
        GreaterThanOrEqual = 12,

        /// <summary>
        /// Nằm trong danh sách giá trị (IN).
        /// </summary>
        In = 13,

        /// <summary>
        /// Không nằm trong danh sách giá trị (NOT IN).
        /// </summary>
        NotIn = 14,

        /// <summary>
        /// Nằm trong khoảng (BETWEEN).
        /// </summary>
        Between = 15
    }

    /// <summary>
    /// Enum định nghĩa các kiểu dữ liệu được hỗ trợ.
    /// </summary>
    public enum DataType
    {
        /// <summary>
        /// Kiểu chuỗi ký tự.
        /// </summary>
        String = 1,

        /// <summary>
        /// Kiểu số (int, long, decimal,...).
        /// </summary>
        Number = 2,

        /// <summary>
        /// Kiểu ngày giờ.
        /// </summary>
        DateTime = 3,

        /// <summary>
        /// Kiểu boolean (true/false).
        /// </summary>
        Boolean = 4,

        /// <summary>
        /// Kiểu GUID / UUID.
        /// </summary>
        Guid = 5,

        /// <summary>
        /// Kiểu chỉ ngày (yyyy-MM-dd), không bao gồm giờ phút giây.
        /// </summary>
        Date = 6
    }
}
