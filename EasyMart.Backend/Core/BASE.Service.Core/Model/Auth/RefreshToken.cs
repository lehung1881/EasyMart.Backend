using BASE.Service.Core.Attribute;
using System.ComponentModel.DataAnnotations;

namespace BASE.Service.Core.Model
{
    [ConfigTable("refresh_token", "")]
    public class RefreshToken : BaseModel
    {
        /// <summary>
        /// Khóa chính
        /// </summary>
        [Key]
        public Guid RefreshTokenID { get; set; }

        /// <summary>
        /// ID người dùng (FK → user.UserID)
        /// </summary>
        public Guid UserID { get; set; }

        /// <summary>
        /// Chuỗi token ngẫu nhiên
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Thời hạn token (mặc định 7 ngày)
        /// </summary>
        public DateTime ExpiresDate { get; set; }

        /// <summary>
        /// Đã thu hồi chưa
        /// Token Rotation: toàn bộ token cũ bị thu hồi khi đăng nhập mới
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// Thời điểm tạo
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Thời điểm thu hồi
        /// </summary>
        public DateTime? RevokedDate { get; set; }
    }
}
