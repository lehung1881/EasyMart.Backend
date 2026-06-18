using BASE.Service.Core.Attribute;
using System.ComponentModel.DataAnnotations;

namespace BASE.Service.Core.Model
{
    [ConfigTable("user", "")]
    public class User : BaseModel
    {
        /// <summary>
        /// Khóa chính
        /// </summary>
        [Key]
        public Guid UserID { get; set; }

        /// <summary>
        /// Email đăng nhập
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Họ và tên
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Mật khẩu mã hóa BCrypt
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Đường dẫn ảnh đại diện
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Trạng thái kích hoạt
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Trạng thái khóa tài khoản
        /// Tự động khóa sau 5 lần đăng nhập sai liên tiếp
        /// </summary>
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// Số lần đăng nhập sai liên tiếp
        /// </summary>
        public int FailedLoginCount { get; set; } = 0;

        /// <summary>
        /// Đã xác thực email chưa
        /// </summary>
        public bool IsEmailVerified { get; set; } = false;

        /// <summary>
        /// Token xác thực email
        /// </summary>
        public string? EmailVerifyToken { get; set; }

        /// <summary>
        /// Thời hạn token xác thực
        /// </summary>
        public DateTime? EmailVerifyExpAt { get; set; }

        /// <summary>
        /// Thời điểm tạo
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Thời điểm cập nhật
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Người tạo
        /// </summary>
        public Guid? CreatedBy { get; set; }

        /// <summary>
        /// Người cập nhật
        /// </summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// Soft delete
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }

    /// <summary>
    /// Thông tin cơ bản của User được trả về sau khi đăng nhập.
    /// Chỉ chứa các thông tin cần thiết, không bao gồm thông tin nhạy cảm như <c>PasswordHash</c>.
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// Định danh duy nhất của User.
        /// Tương ứng với cột <c>UserID</c> trong bảng <c>user</c>.
        /// </summary>
        public Guid UserID { get; set; }

        /// <summary>
        /// Địa chỉ email của User.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Họ và tên đầy đủ của User.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Đường dẫn ảnh đại diện
        /// </summary>
        public string? AvatarUrl { get; set; }

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Khóa chính
        /// </summary>
        public Guid EasyMartID { get; set; }

        /// <summary>
        /// Mã định danh khách hàng (vd: CUST_001)
        /// </summary>
        public string EasyMartCode { get; set; }

        /// <summary>
        /// Tên khách hàng
        /// </summary>
        public string EasyMartName { get; set; }
    }
}
