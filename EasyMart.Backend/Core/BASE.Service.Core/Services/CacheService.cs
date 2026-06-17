using BASE.Service.Core.Model.Cache;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BASE.Service.Core.Services
{
    /// <summary>
    /// lvhung - 17.06.2026
    /// Cache service hỗ trợ resolve key từ config + placeholder substitution
    /// </summary>
    public class CacheService : ICacheService
    {
        // Ngầm định nếu không cấu hình time là 30 phút
        private const int DefaultExpirationSeconds = 1800;

        private readonly IDistributedCache _cache;
        private readonly Dictionary<string, CacheItem> _cacheConfigMap;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public CacheService(IDistributedCache cache, IOptions<CacheConfigOptions> cacheConfigOptions)
        {
            _cache = cache;
            _cacheConfigMap = cacheConfigOptions.Value.CacheItems;
        }

        /// <summary>
        /// lvhung - 17.06.2026
        /// Resolve cache key từ CacheItemName + dictionary placeholder.
        /// VD: "IsConnectedAmisProcess_{TenantID}" + {TenantID: "abc"} => "IsConnectedAmisProcess_abc"
        /// </summary>
        private string ResolveCacheKey(string cacheItemName, Dictionary<string, object>? placeholders = null)
        {
            if (!_cacheConfigMap.TryGetValue(cacheItemName, out var cacheItemConfig))
                throw new InvalidOperationException($"Cache item '{cacheItemName}' không tồn tại trong Cache.json");

            var resolvedKey = cacheItemConfig.KeyPattern;

            if (placeholders is { Count: > 0 })
            {
                foreach (var (placeholderName, placeholderValue) in placeholders)
                {
                    resolvedKey = resolvedKey.Replace(
                        $"{{{placeholderName}}}",
                        placeholderValue?.ToString() ?? string.Empty,
                        StringComparison.OrdinalIgnoreCase
                    );
                }
            }

            return resolvedKey;
        }

        /// <summary>
        /// lvhung - 17.06.2026
        /// Lấy TimeSpan expiration từ config của cache item, fallback về default nếu không tìm thấy
        /// </summary>
        private TimeSpan GetExpiration(string cacheItemName)
        {
            if (_cacheConfigMap.TryGetValue(cacheItemName, out var cacheItemConfig))
                return TimeSpan.FromSeconds(cacheItemConfig.TimeoutInSeconds);

            return TimeSpan.FromSeconds(DefaultExpirationSeconds);
        }

        /// <summary>
        /// Thực hiện đọc dữ liệu từ Distributed Cache thông qua Key đã được xử lý thay thế ký tự placeholder.
        /// Trả về giá trị mặc định của kiểu dữ liệu (null đối với Object) nếu không tìm thấy hoặc cache trống.
        /// </summary>
        public async Task<T?> GetAsync<T>(string cacheItemName, Dictionary<string, object>? placeholders = null)
        {
            var resolvedKey = ResolveCacheKey(cacheItemName, placeholders);
            var json = await _cache.GetStringAsync(resolvedKey);

            if (string.IsNullOrWhiteSpace(json)) return default;

            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        /// <summary>
        /// Thực hiện Serialize đối tượng sang JSON chuỗi và lưu vào Distributed Cache.
        /// Tự động thiết lập cấu hình thời gian hết hạn Absolute Expiration dựa vào cấu hình của từng đầu Cache cụ thể.
        /// </summary>
        public async Task SetAsync<T>(string cacheItemName, T value, Dictionary<string, object>? placeholders = null)
        {
            var resolvedKey = ResolveCacheKey(cacheItemName, placeholders);
            var expiration = GetExpiration(cacheItemName);
            var json = JsonSerializer.Serialize(value, JsonOptions);

            var entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            await _cache.SetStringAsync(resolvedKey, json, entryOptions);
        }

        /// <summary>
        /// Hỗ trợ lấy dữ liệu an toàn kết hợp Double-Check Locking Pattern dựa trên Semaphore độc bản theo Key độc lập.
        /// Ngăn chặn triệt để tình trạng nghẽn cổ chai (Cache Stampede) khi có nhiều request đồng thời gọi dữ liệu gốc bị hụt cache.
        /// </summary>
        public async Task<T> GetOrCreateAsync<T>(string cacheItemName, Func<Task<T>> factory, Dictionary<string, object>? placeholders = null)
        {
            var cachedValue = await GetAsync<T>(cacheItemName, placeholders);
            if (cachedValue is not null) return cachedValue;

            var resolvedKey = ResolveCacheKey(cacheItemName, placeholders);
            var semaphore = Locks.GetOrAdd(resolvedKey, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync();
            try
            {
                // Double-check sau khi acquire lock
                cachedValue = await GetAsync<T>(cacheItemName, placeholders);
                if (cachedValue is not null) return cachedValue;

                var freshValue = await factory();
                await SetAsync(cacheItemName, freshValue, placeholders);
                return freshValue;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Thực hiện xóa bỏ Key được chỉ định ra khỏi Distributed Cache.
        /// Key cần xóa sẽ được tự động resolve chính xác thông qua cấu hình định dạng pattern và placeholder tương ứng.
        /// </summary>
        public async Task RemoveAsync(string cacheItemName, Dictionary<string, object>? placeholders = null)
        {
            var resolvedKey = ResolveCacheKey(cacheItemName, placeholders);
            await _cache.RemoveAsync(resolvedKey);
        }
    }
}