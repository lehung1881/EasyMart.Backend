using BASE.Service.Core.Model;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
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
        /// </summary>
        /// <param name="services">Đối tượng IServiceCollection để đăng ký các dịch vụ.</param>
        /// <param name="config">Đối tượng IConfiguration chứa cấu hình của ứng dụng.</param>
        /// <returns>Trả về IServiceCollection đã được cấu hình xác thực JWT.</returns>
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration config)
        {
            services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null);

            var jwtConfig = GlobalConfig.AppSettings.JwtSettings;
            var key = Encoding.ASCII.GetBytes(jwtConfig.SecretKey);

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidIssuer = jwtConfig.Issuer,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuerSigningKey = true
                };
            });

            return services;
        }
    }
}
