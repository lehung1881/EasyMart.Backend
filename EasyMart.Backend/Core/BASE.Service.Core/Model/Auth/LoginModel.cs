using System;
using System.ComponentModel.DataAnnotations;

namespace BASE.Service.Core.Model
{
    /// <summary>
    /// Model chứa thông tin đầu vào cho yêu cầu đăng nhập.
    /// Được gửi từ client qua <c>POST /api/v1/auth/login</c>.
    /// </summary>
    public class LoginRequest
    {
        /// <summary>
        /// Địa chỉ email dùng để đăng nhập.
        /// Không được để trống và phải đúng định dạng email.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu đăng nhập dạng plain text.
        /// Sẽ được xác minh với hash BCrypt lưu trong database.
        /// Không được để trống và phải có ít nhất 6 ký tự.
        /// </summary>
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Model chứa thông tin trả về sau khi đăng nhập thành công.
    /// Bao gồm Access Token, Refresh Token và thông tin cơ bản của User.
    /// </summary>
    public class LoginResponse
    {
        /// <summary>
        /// JWT Access Token dùng để xác thực các request tiếp theo.
        /// Client cần đính kèm token này vào header: <c>Authorization: Bearer {AccessToken}</c>.
        /// Token có thời hạn ngắn (mặc định: 60 phút).
        /// </summary>
        public string AccessToken { get; set; } = string.Empty;

        /// <summary>
        /// Refresh Token dùng để cấp phát Access Token mới khi hết hạn.
        /// Token có thời hạn dài hơn (mặc định: 7 ngày).
        /// Client cần lưu trữ an toàn (HttpOnly Cookie hoặc Secure Storage).
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Thời điểm hết hạn của Access Token (UTC).
        /// Client có thể dùng giá trị này để chủ động refresh token trước khi hết hạn.
        /// </summary>
        public DateTime ExpiresDate { get; set; }

        /// <summary>
        /// Thông tin cơ bản của User vừa đăng nhập.
        /// </summary>
        public UserInfo User { get; set; } = new();
    }
}
