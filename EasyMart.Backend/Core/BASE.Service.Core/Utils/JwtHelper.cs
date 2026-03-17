using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BASE.Service.Core.Utils
{
    public class JwtHelper
    {
        private readonly IConfiguration _config;

        public JwtHelper(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Tạo Access Token từ thông tin user
        /// </summary>
        public string GenerateAccessToken(UserInfo user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]!;
            var issuer = jwtSettings["Issuer"]!;
            var audience = jwtSettings["Audience"]!;
            var expiresIn = int.Parse(jwtSettings["ExpiresInMinutes"]!);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtClaimKeys.UserID, user.UserID.ToString()),
                new Claim(JwtClaimKeys.Email, user.Email),
                new Claim(JwtClaimKeys.TokenID, Guid.NewGuid().ToString()),
                new Claim(JwtClaimKeys.FullName, user.FullName),
                new Claim(JwtClaimKeys.DatabaseID, user.DatabaseID.ToString()),
                new Claim(JwtClaimKeys.TenantID, user.TenantID.ToString()),
            };


            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresIn),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Tạo Refresh Token ngẫu nhiên
        /// </summary>
        public string GenerateRefreshToken()
        {
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
