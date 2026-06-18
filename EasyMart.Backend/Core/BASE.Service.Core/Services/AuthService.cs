using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Services
{
    /// <summary>
    /// Service dùng để xử lý các thông tin xác thực liên quan đến người dùng hiện tại.
    /// </summary>
    public class AuthService : IAuthService
    {
        /// <summary>
        /// Truy cập thông tin HTTP context hiện tại.
        /// </summary>
        protected readonly IHttpContextAccessor _httpContext;

        /// <summary>
        /// Khởi tạo một instance mới của <see cref="AuthService"/>.
        /// </summary>
        /// <param name="httpContext">IHttpContextAccessor để truy xuất thông tin từ HTTP context.</param>
        public AuthService(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

        /// <summary>
        /// Lấy UserID từ header "X-UserID" trong HTTP request hiện tại.
        /// </summary>
        /// <returns>
        /// Trả về Guid đại diện cho ID của người dùng nếu tồn tại,
        /// ngược lại trả về Guid.Empty.
        /// </returns>
        public Guid GetUserID()
        {
            string userID = _httpContext.HttpContext?.Request?.Headers["X-UserID"];
            if (!string.IsNullOrEmpty(userID))
            {
                return Guid.Parse(userID);
            }
            return Guid.Empty;
        }

        /// <summary>
        /// Lấy EasyMartID từ header "X-EasyMartID" trong HTTP request hiện tại.
        /// </summary>
        /// <returns>
        /// Trả về Guid đại diện cho ID Easymart nếu tồn tại,
        /// ngược lại trả về Guid.Empty.
        /// </returns>
        public Guid GetEasyMartID()
        {
            string easymartID = _httpContext.HttpContext?.Request?.Headers["X-EasyMartID"];
            if (!string.IsNullOrEmpty(easymartID))
            {
                return Guid.Parse(easymartID);
            }
            return Guid.Empty;
        }

        /// <summary>
        /// Lấy FullName từ header "X-FullName" trong HTTP request hiện tại.
        /// Giá trị được decode từ UTF-8 do phía client encode bằng encodeURIComponent.
        /// </summary>
        /// <returns>
        /// Trả về họ tên người dùng nếu tồn tại,
        /// ngược lại trả về chuỗi rỗng.
        /// </returns>
        public string GetFullName()
        {
            string fullName = _httpContext.HttpContext?.Request?.Headers["X-FullName"];
            if (!string.IsNullOrEmpty(fullName))
            {
                return Uri.UnescapeDataString(fullName);
            }
            return string.Empty;
        }
    }
}