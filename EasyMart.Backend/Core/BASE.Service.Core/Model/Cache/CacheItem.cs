using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Model.Cache
{
    /// <summary>
    /// lvhung - 17.06.2026
    /// Cấu hình cho một cache item được đọc từ Cache.json
    /// </summary>
    public class CacheItem
    {
        /// <summary>
        /// Loại cache: 0 = Redis, 1 = MemoryCache
        /// </summary>
        public CacheType Type { get; set; }

        /// <summary>
        /// Pattern của key, có thể chứa placeholder như {EasyMartID}
        /// </summary>
        public string KeyPattern { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian hết hạn (giây)
        /// </summary>
        public int TimeoutInSeconds { get; set; }

        /// <summary>
        /// Mô tả mục đích cache item
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Loại cache
    /// </summary>
    public enum CacheType
    {
        Redis = 0,
        MemoryCache = 1
    }
}
