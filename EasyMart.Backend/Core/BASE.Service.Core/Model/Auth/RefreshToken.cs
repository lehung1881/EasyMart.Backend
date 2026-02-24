using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Model
{
    /// <summary>
    /// Đại diện cho Refresh Token được cấp phát cho User sau khi đăng nhập thành công.
    /// Được ánh xạ tới bảng <c>RefreshTokens</c> trong database.
    /// Refresh Token dùng để cấp phát Access Token mới khi Access Token hết hạn,
    /// mà không cần User đăng nhập lại.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Khóa chính, định danh duy nhất của Refresh Token.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// ID của User sở hữu Refresh Token này.
        /// Khóa ngoại tham chiếu tới bảng <c>Users</c>.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Chuỗi token ngẫu nhiên được sinh ra bằng <see cref="System.Security.Cryptography.RandomNumberGenerator"/>.
        /// Phải là duy nhất trong hệ thống.
        /// </summary>
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm hết hạn của Refresh Token (UTC).
        /// Sau thời điểm này, token không còn hợp lệ dù chưa bị thu hồi.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Trạng thái thu hồi của token.
        /// <c>true</c>: token đã bị thu hồi (do logout, token rotation hoặc bị đánh cắp).
        /// <c>false</c>: token vẫn còn hiệu lực.
        /// </summary>
        public bool IsRevoked { get; set; } = false;

        /// <summary>
        /// Thời điểm tạo Refresh Token (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
