using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Lấy UserID từ header "X-UserID" trong HTTP request hiện tại.
        /// </summary>
        /// <returns>
        /// Trả về Guid đại diện cho ID của người dùng nếu tồn tại,
        /// ngược lại trả về Guid.Empty.
        /// </returns>
        Guid GetUserID();

        /// <summary>
        /// Lấy DatabaseID từ header "X-DatabaseID" trong HTTP request hiện tại.
        /// </summary>
        /// <returns>
        /// Trả về Guid đại diện cho ID database của tenant nếu tồn tại,
        /// ngược lại trả về Guid.Empty.
        /// </returns>
        Guid GetDatabaseID();

        /// <summary>
        /// Lấy TenantID từ header "X-TenantID" trong HTTP request hiện tại.
        /// </summary>
        /// <returns>
        /// Trả về Guid đại diện cho ID tenant nếu tồn tại,
        /// ngược lại trả về Guid.Empty.
        /// </returns>
        Guid GetTenantID();

        /// <summary>
        /// Lấy FullName từ header "X-FullName" trong HTTP request hiện tại.
        /// Giá trị được decode từ UTF-8 do phía client encode bằng encodeURIComponent.
        /// </summary>
        /// <returns>
        /// Trả về họ tên người dùng nếu tồn tại,
        /// ngược lại trả về chuỗi rỗng.
        /// </returns>
        string GetFullName();
    }
}