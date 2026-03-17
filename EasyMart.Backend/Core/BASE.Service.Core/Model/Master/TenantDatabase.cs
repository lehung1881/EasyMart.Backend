using BASE.Service.Core.Attribute;
using System.ComponentModel.DataAnnotations;

namespace BASE.Service.Core.Model
{
    [ConfigTable("tenant_database", "")]
    public class TenantDatabase : BaseModel
    {
        /// <summary>
        /// Khóa chính - Guid dùng trong IMySQLService
        /// </summary>
        [Key]
        public Guid DatabaseID { get; set; }

        /// <summary>
        /// ID khách hàng (không FK - chấp nhận dư thừa)
        /// </summary>
        public Guid TenantID { get; set; }

        /// <summary>
        /// Mã ứng dụng
        /// </summary>
        public string AppCode { get; set; }

        /// <summary>
        /// Địa chỉ host MySQL
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Cổng kết nối
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Tên database
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Tên đăng nhập DB
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// Mật khẩu DB - được mã hóa AES trước khi lưu
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Phiên bản DB hiện tại
        /// </summary>
        public string VersionDB { get; set; }

        /// <summary>
        /// Môi trường (g2, staging, prod...)
        /// </summary>
        public string Env { get; set; } = "g2";

        /// <summary>
        /// Trạng thái: 0 - đang làm việc, 1 - không làm việc
        /// </summary>
        public int Status { get; set; } = 0;

        /// <summary>
        /// Thời điểm tạo
        /// </summary>
        public DateTime? CreatedDate { get; set; }
    }
}
