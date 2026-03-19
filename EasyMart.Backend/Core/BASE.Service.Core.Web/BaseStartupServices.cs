using BASE.Service.Core.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BASE.Service.Core.Web
{
    public class BaseStartupServices
    {
        /// <summary>
        /// Tên policy CORS dùng chung trong toàn bộ ứng dụng.
        /// </summary>
        private const string EasyMartCorsPolicyName = "EasymartCorsPolicy";

        /// <summary>
        /// Đăng ký các service cần thiết cho ứng dụng.
        /// </summary>
        /// <param name="builder">WebApplicationBuilder hiện tại.</param>
        /// <param name="configuration">IConfiguration của ứng dụng.</param>
        /// <param name="isSetAuthorization">Có cấu hình JWT authentication không.</param>
        public static void ConfigureServices(WebApplicationBuilder builder, IConfiguration configuration = null, bool isSetAuthorization = true)
        {
            // Khởi tạo cấu hình vào configuration — phải chạy trước để GlobalConfig có dữ liệu
            InitConfigGlobal(builder);

            // Thêm option này để bỏ validate model ở đầu controller
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            // Xử lý file upload lớn
            builder.Services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = 60000000;
            });

            // Cấu hình để khi convert json không bị chuyển sang camelCase
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = null;
            });

            // Đăng ký CORS với named policy.
            // AllowAnyOrigin() không được dùng kèm AllowCredentials() — vi phạm CORS protocol.
            // Với service nội bộ không khai báo AllowedOrigins thì CORS không được áp dụng.
            var allowedOrigins = GlobalConfig.AppSettings.AllowedOrigins;
            builder.Services.AddCors(options =>
            {
                if (allowedOrigins != null && allowedOrigins.Length > 0)
                {
                    options.AddPolicy(EasyMartCorsPolicyName, policy => policy
                        .WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                    );
                }
            });

            // Cấu hình JWT — đọc token từ Authorization header (Bearer token)
            if (isSetAuthorization && configuration != null)
            {
                builder.Services.AddJwtAuthentication(configuration);
            }

            // HttpContext
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Add core Services
            builder.Services.UseCoreServices();

            // Add BL Services
            // builder.Services.UseCoreBLServices();

            // Add services
            builder.Services.UseWebCoreServices(configuration);
        }

        /// <summary>
        /// Cấu hình middleware pipeline cho ứng dụng.
        /// Thứ tự middleware rất quan trọng — không được thay đổi.
        /// </summary>
        /// <param name="app">WebApplication hiện tại.</param>
        public static void ConfigureApp(WebApplication app)
        {
            app.UseHttpsRedirection();

            // CORS phải đứng trước UseAuthentication và UseAuthorization
            var allowedOrigins = GlobalConfig.AppSettings.AllowedOrigins;
            if (allowedOrigins != null && allowedOrigins.Length > 0)
            {
                app.UseCors(EasyMartCorsPolicyName);
            }

            // Xử lý Authentication và Authorization
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }

        /// <summary>
        /// Khởi tạo cấu hình toàn cục từ file appsettings.json.
        /// Tìm file theo cấu trúc thư mục, tối đa 15 cấp cha.
        /// </summary>
        /// <param name="builder">WebApplicationBuilder hiện tại.</param>
        public static void InitConfigGlobal(WebApplicationBuilder builder)
        {
            try
            {
                string pathConfig = GetPathConfigCommon(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "appsettings.json");

                if (string.IsNullOrEmpty(pathConfig))
                {
                    Console.WriteLine("BaseStartupServices. InitConfigGlobal: Không tìm thấy file appsettings.json trong thư mục config. Sử dụng cấu hình mặc định.");
                    var configGlobal = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
                    GlobalConfig.InitConfig(configGlobal);
                    return;
                }

                builder.Configuration.AddJsonFile(pathConfig, optional: false, reloadOnChange: true);
                Console.WriteLine($"BaseStartupServices. InitConfigGlobal: Đã load config từ <{pathConfig}>.");

                var config = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings();
                GlobalConfig.InitConfig(config);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BaseStartupServices. InitConfigGlobal Exception: <{ex}>.");
            }
        }

        /// <summary>
        /// Tìm đường dẫn đầy đủ đến file cấu hình bằng cách duyệt ngược lên thư mục cha.
        /// File được tìm trong thư mục con "config" tại mỗi cấp, tối đa 15 cấp.
        /// </summary>
        /// <param name="folderRoot">Thư mục bắt đầu tìm kiếm.</param>
        /// <param name="fileName">Tên file cần tìm, ví dụ: appsettings.json.</param>
        /// <returns>Đường dẫn đầy đủ đến file nếu tìm thấy, ngược lại trả về chuỗi rỗng.</returns>
        private static string GetPathConfigCommon(string folderRoot, string fileName)
        {
            var configCommon = string.Empty;

            try
            {
                Console.WriteLine($"GetPathConfigCommon. folderRoot: <{folderRoot}>; fileName: <{fileName}>.");

                var countCheck = 0;
                var existFile = false;

                do
                {
                    configCommon = Path.Combine(folderRoot, "config", fileName);

                    Console.WriteLine($"GetPathConfigCommon. configCommon path: {configCommon}");

                    if (!File.Exists(configCommon))
                    {
                        countCheck++;
                        configCommon = string.Empty;
                        var parent = Directory.GetParent(folderRoot);
                        if (parent == null) break;
                        folderRoot = parent.FullName;
                    }
                    else
                    {
                        existFile = true;
                    }
                }
                while (!existFile && countCheck < 15);

                if (!existFile)
                {
                    Console.WriteLine($"GetPathConfigCommon. Không tìm thấy file <{fileName}> sau {countCheck} lần kiểm tra.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetPathConfigCommon Exception: {ex.Message}");
            }

            return configCommon;
        }
    }
}