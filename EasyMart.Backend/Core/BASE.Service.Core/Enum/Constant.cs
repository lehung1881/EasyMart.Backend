using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Enum
{
    public class Constants
    {
        /// <summary>
        /// ID của Master Database, sử dụng <see cref="Guid.Empty"/> làm định danh mặc định.
        /// </summary>
        public static readonly Guid MasterDatabaseID = Guid.Parse("00000000-0000-0000-0000-000000000001");
    }

    public static class JwtClaimKeys
    {
        public const string UserID = "user_id";
        public const string Email = "email";
        public const string TokenID = "jti";
        public const string FullName = "full_name";
        public const string DatabaseID = "database_id";
        public const string TenantID = "tenant_id";
    }
}
