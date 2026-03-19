using BASE.Service.Core.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace BASE.Service.Core.Web
{
    /// <summary>
    /// Lớp mở rộng để cấu hình xác thực JWT cho ứng dụng ASP.NET Core.
    /// </summary>
    public static class JwtAuthorizationExtensions
    {
        /// <summary>
        /// Cấu hình dịch vụ xác thực bằng JWT cho ứng dụng.
        /// Access Token được đọc từ Authorization header theo chuẩn Bearer token.
        /// Token được lưu ở localStorage phía client và gửi lên qua header mỗi request.
        /// Encoding UTF8 được dùng nhất quán giữa lúc ký token (JwtHelper) và lúc validate.
        /// </summary>
        /// <param name="services">Đối tượng IServiceCollection để đăng ký các dịch vụ.</param>
        /// <param name="config">Đối tượng IConfiguration chứa cấu hình của ứng dụng.</param>
        /// <returns>Trả về IServiceCollection đã được cấu hình xác thực JWT.</returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
            {
                // Đọc config bên trong callback để đảm bảo GlobalConfig đã được init xong
                var jwtConfig = GlobalConfig.AppSettings.JwtSettings;
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey));

                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtConfig.Issuer,
                    ValidAudience = jwtConfig.Audience,
                    IssuerSigningKey = key,

                    ClockSkew = TimeSpan.Zero
                };

                option.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Auth failed: {context.Exception.GetType().Name} - {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully!");
                        return Task.CompletedTask;
                    },
                    OnMessageReceived = context =>
                    {
                        var auth = context.Request.Headers["Authorization"].ToString();
                        Console.WriteLine($"RAW HEADER: {auth}");
                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }
    }
}