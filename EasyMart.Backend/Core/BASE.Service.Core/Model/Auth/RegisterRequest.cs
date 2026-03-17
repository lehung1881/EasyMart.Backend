namespace BASE.Service.Core.Model
{
    /// <summary>
    /// Request model cho API đăng ký tài khoản mới.
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>
        /// Email đăng nhập - phải là email hợp lệ và chưa tồn tại trong hệ thống.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Họ và tên đầy đủ.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu - sẽ được mã hóa BCrypt trước khi lưu.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Số điện thoại (không bắt buộc).
        /// </summary>
        public string? PhoneNumber { get; set; }

        /// <summary>
        /// Tên công ty / tổ chức.
        /// </summary>
        public string TenantName { get; set; } = string.Empty;

        /// <summary>
        /// Mã số thuế của công ty / tổ chức.
        /// </summary>
        public string TaxCode { get; set; } = string.Empty;
    }
}