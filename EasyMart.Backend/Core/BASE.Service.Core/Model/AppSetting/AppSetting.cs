using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Model
{
    /// <summary>
    /// Cấu hình chung của ứng dụng, bind từ section "AppSettings" trong appsettings.json
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Cấu hình các chuỗi kết nối database
        /// </summary>
        public ConnectionStringsConfig ConnectionStrings { get; set; }

        /// <summary>
        /// Cấu hình JWT Authentication (token, issuer, audience,...)
        /// </summary>
        public JwtSettingsConfig JwtSettings { get; set; }

        /// <summary>
        /// Cấu hình Cloudinary (dịch vụ lưu trữ & quản lý hình ảnh)
        /// </summary>
        public ClouldinaryConfig ClouldinaryConfig { get; set; }

        /// <summary>
        /// Định db mẫu khi tạo mới dữ liệu
        /// </summary>
        public string DatabaseNameTemplate { get; set; } = "easymart_{0}_{1}";
    }

    /// <summary>
    /// Cấu hình chuỗi kết nối đến các database
    /// </summary>
    public class ConnectionStringsConfig
    {
        /// <summary>
        /// Chuỗi kết nối đến database MasterDB
        /// </summary>
        public string MasterDB { get; set; }

        /// <summary>
        /// Chuỗi kết nối đến database tạo mới dữ liệu
        /// </summary>
        public string TemplateDB { get; set; }
    }

    /// <summary>
    /// Cấu hình JWT (JSON Web Token) cho xác thực & phân quyền
    /// </summary>
    public class JwtSettingsConfig
    {
        /// <summary>
        /// Khóa bí mật dùng để ký và xác thực token
        /// </summary>
        public string SecretKey { get; set; }

        /// <summary>
        /// Tên nhà phát hành token (Issuer)
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// Đối tượng nhận token (Audience) - thường là URL của ứng dụng
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Thời gian hết hạn của token (đơn vị: phút)
        /// </summary>
        public int ExpiresInMinutes { get; set; }
    }

    /// <summary>
    /// Cấu hình kết nối đến dịch vụ Cloudinary (upload & quản lý media)
    /// </summary>
    public class ClouldinaryConfig
    {
        /// <summary>
        /// Tên cloud trên Cloudinary
        /// </summary>
        public string CloudName { get; set; }

        /// <summary>
        /// API Key để xác thực với Cloudinary
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// API Secret để xác thực với Cloudinary (cần bảo mật)
        /// </summary>
        public string ApiSecret { get; set; }
    }
}
