using BASE.Service.Core.Attribute;
using System.ComponentModel.DataAnnotations;

namespace BASE.Service.Core.Model
{
    /// <summary>
    /// Model thực thể đại diện cho cấu hình Database riêng biệt của từng Siêu thị.
    /// Ánh xạ trực tiếp với bảng "easymart_db_config" trong cơ sở dữ liệu.
    /// </summary>
    [ConfigTable("easymart_db_config", "")]
    public class EasyMartDbConfig : BaseModel
    {
        /// <summary>
        /// Khóa chính - Số tự tăng (INT AUTO_INCREMENT)
        /// </summary>
        [Key]
        public int DbConfigID { get; set; }

        /// <summary>
        /// ID Siêu thị / Khách hàng (Không FK vật lý - quản lý logic qua code)
        /// </summary>
        public Guid EasyMartID { get; set; }

        /// <summary>
        /// Địa chỉ host kết nối MySQL Server
        /// </summary>
        public string Server { get; set; }

        /// <summary>
        /// Cổng kết nối DB Server (Ví dụ: 3306)
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Tên cơ sở dữ liệu vật lý
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Tên tài khoản đăng nhập DB
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// Mật khẩu kết nối DB - đã được mã hóa AES bảo mật trước khi lưu
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Phiên bản DB hiện tại (phục vụ migration cấu trúc)
        /// </summary>
        public string VersionDB { get; set; }

        /// <summary>
        /// Môi trường dữ liệu vận hành (g2, dev, staging, prod...)
        /// </summary>
        public string Env { get; set; } = "g2";

        /// <summary>
        /// Trạng thái hoạt động: 0 - Đang làm việc, 1 - Không làm việc
        /// </summary>
        public int Status { get; set; } = 0;

        /// <summary>
        /// Thời điểm khởi tạo bản ghi cấu hình
        /// </summary>
        public DateTime? CreatedDate { get; set; }
    }
}