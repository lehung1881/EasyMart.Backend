using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Model
{
    /// <summary>
    /// Đại diện cho thông tin tài khoản người dùng trong hệ thống.
    /// Được ánh xạ tới bảng <c>Users</c> trong database.
    /// </summary>
    public class User : BaseModel
    {
        /// <summary>
        /// Khóa chính, định danh duy nhất của User.
        /// </summary>
        public Guid UserID { get; set; }

        /// <summary>
        /// Địa chỉ email của User, dùng làm tên đăng nhập.
        /// Phải là duy nhất trong hệ thống.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Họ và tên đầy đủ của User.
        /// </summary>
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Mật khẩu đã được mã hóa bằng BCrypt.
        /// Không bao giờ lưu mật khẩu dạng plain text.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Vai trò của User trong hệ thống (ví dụ: <c>"Admin"</c>, <c>"User"</c>).
        /// Mặc định là <c>"User"</c>.
        /// </summary>
        public string Role { get; set; } = "User";

        /// <summary>
        /// Trạng thái kích hoạt tài khoản.
        /// <c>true</c>: tài khoản đã được kích hoạt (verify email).
        /// <c>false</c>: tài khoản chưa được kích hoạt.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Trạng thái khóa tài khoản.
        /// <c>true</c>: tài khoản bị khóa (do đăng nhập sai quá nhiều lần hoặc bị admin khóa).
        /// <c>false</c>: tài khoản hoạt động bình thường.
        /// </summary>
        public bool IsLocked { get; set; } = false;

        /// <summary>
        /// Số lần đăng nhập sai liên tiếp.
        /// Tài khoản sẽ bị khóa khi giá trị này đạt ngưỡng cho phép (mặc định: 5 lần).
        /// Được reset về <c>0</c> khi đăng nhập thành công.
        /// </summary>
        public int FailedLoginCount { get; set; } = 0;

        /// <summary>
        /// Thời điểm tạo tài khoản (UTC).
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
