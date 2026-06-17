using BASE.Service.Core.Model;
using BASE.Service.Core.Services;
using EasyMart.DLBase;
using EasyMart.Model.System;

namespace EasyMart.DL.System
{
    public class DLUser : DLBaseEasyMart
    {
        public DLUser(IMySQLService databaseService) : base(databaseService)
        {
        }

        /// <summary>
        /// Thực hiện truy vấn vào Database của Tenant để lấy danh sách ánh xạ quyền của User
        /// </summary>
        public async Task<List<SysMscPermissionMapping>> BaseGetPermissionFromDbAsync(Guid tenantID, Guid userID)
        {
            // 1. Định nghĩa câu lệnh SQL (Lấy các trường tương ứng với Property của Class nhận dữ liệu)
            const string commandText = @"
                SELECT b.SubSystemCode, b.ListPermission 
                FROM sys_msc_user a 
                INNER JOIN sys_msc_role_permission_mapping b ON a.RoleID = b.RoleID 
                WHERE a.UserID = @UserID;";

            // 2. Nạp tham số cho câu lệnh truy vấn
            var parameters = new Dictionary<string, object>
            {
                { "@UserID", userID }
            };

            // 3. Thực thi truy vấn qua DatabaseService (tenantID đóng vai trò là tenantID)
            var result = await _databaseService.QueryUsingCommandText<SysMscPermissionMapping>(tenantID, commandText, parameters);

            return result ?? new List<SysMscPermissionMapping>();
        }
    }
}
