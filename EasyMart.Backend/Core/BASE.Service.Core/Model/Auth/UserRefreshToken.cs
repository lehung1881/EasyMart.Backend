using BASE.Service.Core.Attribute;
using System.ComponentModel.DataAnnotations;

namespace BASE.Service.Core.Model
{
    /// <summary>
    /// Model thực thể đại diện cho mã Refresh Token của người dùng (JWT Authentication).
    /// Ánh xạ trực tiếp với bảng "user_refresh_token" trong cơ sở dữ liệu.
    /// </summary>
    [ConfigTable("user_refresh_token", "")]
    public class UserRefreshToken : BaseModel
    {
        /// <summary>
        /// Khóa chính định danh phiên Refresh Token
        /// </summary>
        [Key]
        public Guid RefreshTokenID { get; set; }

        /// <summary>
        /// ID người dùng sở hữu token (Liên kết tới bảng user)
        /// </summary>
        public Guid UserID { get; set; }

        /// <summary>
        /// Chuỗi mã Token ngẫu nhiên bảo mật
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Thời hạn hết hạn của token (Mặc định thường là 7 ngày)
        /// </summary>
        public DateTime ExpiresDate { get; set; }

        /// <summary>
        /// Trạng thái thu hồi: 1 - Đã bị hủy/vô hiệu hóa sớm, 0 - Còn hiệu lực
        /// Phục vụ cơ chế Token Rotation (Thu hồi toàn bộ token cũ khi cấp token mới)
        /// </summary>
        public int IsRevoked { get; set; } = 0;

        /// <summary>
        /// Thời điểm khởi tạo token
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Thời điểm token bị thu hồi (nếu có)
        /// </summary>
        public DateTime? RevokedDate { get; set; }
    }
}