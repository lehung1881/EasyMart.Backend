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
        public JwtHelper()
        {
        }

        /// <summary>
        /// Tạo Access Token từ thông tin user
        /// </summary>
        public string GenerateAccessToken(UserInfo user)
        {
            var jwtConfig = GlobalConfig.AppSettings.JwtSettings;

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtClaimKeys.UserID, user.UserID.ToString()),
                new Claim(JwtClaimKeys.Email, user.Email),
                new Claim(JwtClaimKeys.TokenID, Guid.NewGuid().ToString()),
                new Claim(JwtClaimKeys.FullName, user.FullName),
                new Claim(JwtClaimKeys.EasyMartID, user.EasyMartID.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: jwtConfig.Issuer,
                audience: jwtConfig.Audience,
                claims: claims,
                expires: DateTime.Now.AddSeconds(jwtConfig.AccessTokenExpires),
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
