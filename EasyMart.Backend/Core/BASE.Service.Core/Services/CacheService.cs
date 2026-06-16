using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BASE.Service.Core.Services
{
    public class CacheService : ICacheService
    {
        /// <summary>
        /// Thời gian cache mặc định (phút).
        /// </summary>
        private const int DefaultExpirationMinutes = 30;

        /// <summary>
        /// Cache phân tán.
        /// </summary>
        private readonly IDistributedCache _cache;

        /// <summary>
        /// Danh sách lock dùng để chống cache stampede.
        /// </summary>
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();

        /// <summary>
        /// Json serializer options.
        /// </summary>
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public CacheService(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <inheritdoc />
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var json = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }

        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow =
                    expiration ??
                    TimeSpan.FromMinutes(DefaultExpirationMinutes)
            };

            await _cache.SetStringAsync(key, json, options, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }

        /// <inheritdoc />
        public async Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
        {
            foreach (var key in keys)
            {
                await _cache.RemoveAsync(key, cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            var value = await _cache.GetAsync(key, cancellationToken);

            return value is not null;
        }

        /// <inheritdoc />
        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var cachedValue = await GetAsync<T>(key, cancellationToken);

            if (cachedValue is not null)
            {
                return cachedValue;
            }

            var semaphore = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync(cancellationToken);

            try
            {
                cachedValue = await GetAsync<T>(key, cancellationToken);

                if (cachedValue is not null)
                {
                    return cachedValue;
                }

                var value = await factory();

                await SetAsync(key, value, expiration, cancellationToken);

                return value;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
