using BASE.Service.Core.Attribute;
using System.ComponentModel.DataAnnotations;

namespace BASE.Service.Core.Model
{
    [ConfigTable("user_login_history", "")]
    public class UserLoginHistory : BaseModel
    {
        /// <summary>
        /// Khóa chính
        /// </summary>
        [Key]
        public Guid LoginHistoryID { get; set; }

        /// <summary>
        /// ID người dùng (FK → user.UserID)
        /// </summary>
        public Guid UserID { get; set; }

        /// <summary>
        /// Địa chỉ IP (hỗ trợ IPv6, tối đa 45 ký tự)
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Thông tin trình duyệt/thiết bị
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Đăng nhập thành công hay không
        /// </summary>
        public bool IsSuccess { get; set; } = true;

        /// <summary>
        /// Lý do thất bại (nếu có)
        /// </summary>
        public string? FailReason { get; set; }

        /// <summary>
        /// Thời điểm đăng nhập
        /// </summary>
        public DateTime LoginAt { get; set; }
    }
}
