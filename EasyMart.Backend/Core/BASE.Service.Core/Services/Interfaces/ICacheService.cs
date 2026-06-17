using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BASE.Service.Core.Services
{
    /// <summary>
    /// Interface định nghĩa các phương thức tương tác với hệ thống Cache (Hỗ trợ cấu hình động và placeholder)
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Lấy dữ liệu từ cache dựa trên tên cấu hình và danh sách tham số truyền vào.
        /// </summary>
        Task<T?> GetAsync<T>(string cacheItemName, Dictionary<string, object>? placeholders = null);

        /// <summary>
        /// Ghi dữ liệu vào cache dựa trên cấu hình, tự động tính toán thời gian hết hạn (TTL).
        /// </summary>
        Task SetAsync<T>(string cacheItemName, T value, Dictionary<string, object>? placeholders = null);

        /// <summary>
        /// Lấy dữ liệu từ cache. Nếu không tồn tại, thực thi hàm factory để lấy dữ liệu gốc, ghi lại vào cache và trả về (Có cơ chế chống Cache Stampede / Cache Avalanche).
        /// </summary>
        Task<T> GetOrCreateAsync<T>(string cacheItemName, Func<Task<T>> factory, Dictionary<string, object>? placeholders = null);

        /// <summary>
        /// Xóa dữ liệu trong cache dựa trên tên cấu hình và tham số truyền vào.
        /// </summary>
        Task RemoveAsync(string cacheItemName, Dictionary<string, object>? placeholders = null);
    }
}