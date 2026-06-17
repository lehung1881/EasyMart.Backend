using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Model.Cache
{
    /// <summary>
    /// lvhung - 17.06.2026
    /// Root options class mapping toàn bộ Cache.json
    /// </summary>
    public class CacheConfigOptions
    {
        public const string SectionName = "CacheItems";

        /// <summary>
        /// Key = tên cache item (trùng với CacheItemName const)
        /// </summary>
        public Dictionary<string, CacheItem> CacheItems { get; set; } = new();
    }
}
