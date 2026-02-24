using BCrypt.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Utils
{
    public class CommonFn
    {
        /// <summary>
        /// Mã hóa mật khẩu sử dụng BCrypt
        /// </summary>
        /// <param name="password">Mật khẩu cần mã hóa</param>
        /// <returns>Mật khẩu đã được hash</returns>
        public static string HashPassword(string password)
        {
            // WorkFactor 12 là mức độ bảo mật khuyến nghị (2^12 iterations)
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        /// <summary>
        /// Xác thực mật khẩu
        /// </summary>
        /// <param name="password">Mật khẩu người dùng nhập</param>
        /// <param name="hashedPassword">Mật khẩu đã hash trong database</param>
        /// <returns>True nếu khớp, False nếu không</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
