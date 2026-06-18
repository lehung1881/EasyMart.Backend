using BASE.Service.Core.BL;
using EasyMart.BLBase;
using EasyMart.Constant.Constant;
using EasyMart.DL.System;
using EasyMart.Model.System;

namespace EasyMart.BL.System
{
    public class BLUser : BLBaseEasyMart<DLUser>
    {
        public BLUser(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override DLUser CreateDL()
        {
            return new DLUser(_mySQLService);
        }

        /// <summary>
        /// Hàm xử lý logic nghiệp vụ lấy quyền của User
        /// </summary>
        public async Task<List<SysMscPermissionMapping>> GetPermissionUserAsync()
        {
            // 1. Định nghĩa các tham số
            var placeholders = new Dictionary<string, object>
            {
                { "EasyMartID", EasyMartID },
                { "UserID", UserID }
            };

            // 2. Gọi GetOrCreateAsync: Nếu có cache thì lấy luôn, nếu chưa có thì chạy hàm lambda bên dưới
            var userPermission = await _cacheService.GetOrCreateAsync(
                CacheItemName.UserPermission,
                () => BaseGetPermissionFromDbAsync(EasyMartID, UserID),
                placeholders
            );

            return userPermission;
        }

        /// <summary>
        /// Hàm gốc chọc vào Database lấy dữ liệu (Chỉ bị gọi khi hụt cache)
        /// </summary>
        private async Task<List<SysMscPermissionMapping>> BaseGetPermissionFromDbAsync(Guid easymartID, Guid userID)
        {
            return await DLObject.BaseGetPermissionFromDbAsync(easymartID, userID);
        }
    }
}
