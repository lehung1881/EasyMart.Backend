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


    public enum ModelState : int
    {
        Insert = 0,

        Update = 1,

        Delete = 2,
    }

    public enum EnumFilterCondition : int
    {
        Empty = 1,
        NotEmpty = 2,
        Equal = 3,
        NotEqual = 4,
        Contain = 5,
        NotContain = 6,
        GreaterThan = 7,
        LessThan = 8,
        GreaterThanEqual = 9,
        LessThanEqual = 10
    }

    public enum EnumDataType : int
    {
        Text = 1,
        Date = 2,
        DateTime = 3,
        Boolean = 4,
        Radio = 5,
        Password = 6,
    }
}
