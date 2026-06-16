using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Services
{
    public interface ICacheService
    {
        /// <summary>
        /// Lấy dữ liệu từ cache theo key.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu cần lấy.</typeparam>
        /// <param name="key">Khóa cache.</param>
        /// <param name="cancellationToken">Token hủy tác vụ.</param>
        /// <returns>Dữ liệu cache hoặc null nếu không tồn tại.</returns>
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lưu dữ liệu vào cache.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu cần lưu.</typeparam>
        /// <param name="key">Khóa cache.</param>
        /// <param name="value">Dữ liệu cần lưu.</param>
        /// <param name="expiration">Thời gian hết hạn.</param>
        /// <param name="cancellationToken">Token hủy tác vụ.</param>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa dữ liệu cache theo key.
        /// </summary>
        /// <param name="key">Khóa cache.</param>
        /// <param name="cancellationToken">Token hủy tác vụ.</param>
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Xóa nhiều cache theo danh sách key.
        /// </summary>
        /// <param name="keys">Danh sách khóa cache.</param>
        /// <param name="cancellationToken">Token hủy tác vụ.</param>
        Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

        /// <summary>
        /// Kiểm tra cache có tồn tại hay không.
        /// </summary>
        /// <param name="key">Khóa cache.</param>
        /// <param name="cancellationToken">Token hủy tác vụ.</param>
        /// <returns>True nếu tồn tại, ngược lại False.</returns>
        Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

        /// <summary>
        /// Lấy dữ liệu từ cache, nếu chưa có thì tạo mới và lưu cache.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu.</typeparam>
        /// <param name="key">Khóa cache.</param>
        /// <param name="factory">Hàm lấy dữ liệu từ nguồn gốc.</param>
        /// <param name="expiration">Thời gian hết hạn.</param>
        /// <param name="cancellationToken">Token hủy tác vụ.</param>
        /// <returns>Dữ liệu cache.</returns>
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    }
}
