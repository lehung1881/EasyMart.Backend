using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using BASE.Service.Core.Services;
using EasyMart.DLBase;
using System.Data;

namespace EasyMart.DL.Auth
{
    /// <summary>
    /// Data Layer xử lý các thao tác liên quan đến xác thực (Authentication).
    /// Bao gồm: quản lý thông tin User, Refresh Token, Tenant, TenantUser và TenantDatabase.
    /// </summary>
    public class DLAuth : DLBaseEasyMart
    {
        #region Constructor

        /// <summary>
        /// Khởi tạo <see cref="DLAuth"/> với dịch vụ kết nối MySQL.
        /// </summary>
        /// <param name="databaseService">Dịch vụ thao tác với MySQL database.</param>
        public DLAuth(IMySQLService databaseService) : base(databaseService)
        {
        }

        #endregion

        #region User

        /// <summary>
        /// Kiểm tra email đã tồn tại trong hệ thống chưa.
        /// </summary>
        /// <param name="email">Địa chỉ email cần kiểm tra.</param>
        /// <returns><c>true</c> nếu email đã tồn tại; <c>false</c> nếu chưa.</returns>
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            var sql = "SELECT UserID FROM user WHERE LOWER(Email) = LOWER(@Email) AND IsDeleted = 0 LIMIT 1";

            var parameters = new Dictionary<string, object>
            {
                { "@Email", email }
            };

            var result = await _databaseService.QueryUsingCommandText<User>(Constants.MasterDatabaseID, sql, parameters);
            return result?.Count > 0;
        }

        /// <summary>
        /// Lấy thông tin User theo địa chỉ email (không phân biệt hoa thường).
        /// </summary>
        /// <param name="email">Địa chỉ email của User cần tìm.</param>
        /// <returns>
        /// Đối tượng <see cref="User"/> nếu tìm thấy; <c>null</c> nếu không tồn tại.
        /// </returns>
        public async Task<User> GetByEmailAsync(string email)
        {
            var sql = "SELECT * FROM user WHERE LOWER(Email) = LOWER(@Email) AND IsDeleted = 0 LIMIT 1";

            var parameters = new Dictionary<string, object>
            {
                { "@Email", email }
            };

            var result = await _databaseService.QueryUsingCommandText<User>(Constants.MasterDatabaseID, sql, parameters);
            return result?.FirstOrDefault();
        }

        /// <summary>
        /// Lấy thông tin <see cref="UserInfo"/> theo <paramref name="userID"/>
        /// bằng cách join bảng <c>user</c>, <c>tenant_user</c> và <c>tenant</c>.
        /// </summary>
        /// <param name="userID">Định danh duy nhất của người dùng cần truy vấn.</param>
        /// <returns>
        /// <see cref="UserInfo"/> nếu tìm thấy; <c>null</c> nếu không tồn tại hoặc đã bị xóa.
        /// </returns>
        public async Task<UserInfo> GetUserInfoByIDAsync(Guid userID)
        {
            var sql = @"
                SELECT 
                    u.UserID,
                    ut.DatabaseID,
                    u.Email,
                    u.FullName,
                    u.AvatarUrl,
                    u.PhoneNumber,
                    t.TenantID,
                    t.TenantCode,
                    t.TenantName
                FROM user u
                LEFT JOIN tenant_user ut ON u.UserID = ut.UserID
                LEFT JOIN tenant t ON ut.TenantID = t.TenantID
                WHERE u.UserID = @UserID
                  AND u.IsDeleted = 0
                LIMIT 1";

            var parameters = new Dictionary<string, object>
            {
                { "@UserID", userID }
            };

            var result = await _databaseService.QueryUsingCommandText<UserInfo>(Constants.MasterDatabaseID, sql, parameters);
            return result?.FirstOrDefault();
        }

        /// <summary>
        /// Lưu User mới vào database trong một transaction có sẵn.
        /// </summary>
        /// <param name="user">Đối tượng User cần lưu.</param>
        /// <param name="cnn">Connection đang mở.</param>
        /// <param name="tran">Transaction đang hoạt động.</param>
        /// <returns><c>true</c> nếu lưu thành công; <c>false</c> nếu thất bại.</returns>
        public async Task<bool> SaveUserAsync(User user, IDbConnection cnn, IDbTransaction tran)
        {
            var sql = @"
                INSERT INTO user
                    (UserID, Email, FullName, PasswordHash, PhoneNumber,
                     IsActive, IsLocked, FailedLoginCount, IsEmailVerified, IsDeleted)
                VALUES
                    (@UserID, @Email, @FullName, @PasswordHash, @PhoneNumber,
                     @IsActive, @IsLocked, @FailedLoginCount, @IsEmailVerified, @IsDeleted)";

            var parameters = new Dictionary<string, object>
            {
                { "@UserID",           user.UserID },
                { "@Email",            user.Email },
                { "@FullName",         user.FullName },
                { "@PasswordHash",     user.PasswordHash },
                { "@PhoneNumber",      user.PhoneNumber ?? (object)DBNull.Value },
                { "@IsActive",         user.IsActive },
                { "@IsLocked",         user.IsLocked },
                { "@FailedLoginCount", user.FailedLoginCount },
                { "@IsEmailVerified",  user.IsEmailVerified },
                { "@IsDeleted",        user.IsDeleted },
            };

            return await _databaseService.ExecuteUsingCommandText(cnn, tran, sql, parameters);
        }

        /// <summary>
        /// Cập nhật thông tin xác thực của User bao gồm:
        /// số lần đăng nhập sai (<see cref="User.FailedLoginCount"/>),
        /// trạng thái khóa (<see cref="User.IsLocked"/>),
        /// và trạng thái kích hoạt (<see cref="User.IsActive"/>).
        /// </summary>
        /// <param name="user">Đối tượng User chứa thông tin cần cập nhật.</param>
        /// <returns><c>true</c> nếu cập nhật thành công; <c>false</c> nếu thất bại.</returns>
        public async Task<bool> UpdateAsync(User user)
        {
            var sql = @"
                UPDATE user SET
                    FailedLoginCount = @FailedLoginCount,
                    IsLocked         = @IsLocked,
                    IsActive         = @IsActive
                WHERE UserID = @UserID";

            var parameters = new Dictionary<string, object>
            {
                { "@UserID",           user.UserID },
                { "@FailedLoginCount", user.FailedLoginCount },
                { "@IsLocked",         user.IsLocked },
                { "@IsActive",         user.IsActive },
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterDatabaseID, sql, parameters);
        }

        #endregion

        #region Refresh Token

        /// <summary>
        /// Lấy thông tin Refresh Token từ database để xác thực.
        /// Chỉ trả về token còn hiệu lực (chưa bị thu hồi).
        /// </summary>
        /// <param name="token">Chuỗi Refresh Token cần tìm kiếm.</param>
        /// <returns>
        /// Đối tượng <see cref="RefreshToken"/> nếu token hợp lệ và chưa bị thu hồi;
        /// <c>null</c> nếu không tìm thấy hoặc đã bị thu hồi.
        /// </returns>
        public async Task<RefreshToken> GetRefreshTokenAsync(string token)
        {
            var sql = @"
                SELECT * FROM refresh_token 
                WHERE Token = @Token AND IsRevoked = 0 
                LIMIT 1";

            var parameters = new Dictionary<string, object>
            {
                { "@Token", token }
            };

            var result = await _databaseService.QueryUsingCommandText<RefreshToken>(Constants.MasterDatabaseID, sql, parameters);
            return result?.FirstOrDefault();
        }

        /// <summary>
        /// Kiểm tra Refresh Token có hợp lệ và chưa hết hạn không.
        /// Chỉ trả về token còn hiệu lực (chưa bị thu hồi và chưa hết hạn).
        /// </summary>
        /// <param name="token">Chuỗi Refresh Token cần kiểm tra.</param>
        /// <returns>
        /// Đối tượng <see cref="RefreshToken"/> nếu hợp lệ;
        /// <c>null</c> nếu không tìm thấy, đã bị thu hồi hoặc đã hết hạn.
        /// </returns>
        public async Task<RefreshToken> GetValidRefreshTokenAsync(string token)
        {
            var sql = @"
                SELECT * FROM refresh_token 
                WHERE Token = @Token 
                  AND IsRevoked = 0 
                  AND ExpiresDate > @Now
                LIMIT 1";

            var parameters = new Dictionary<string, object>
            {
                { "@Token", token },
                { "@Now",   DateTime.UtcNow },
            };

            var result = await _databaseService.QueryUsingCommandText<RefreshToken>(Constants.MasterDatabaseID, sql, parameters);
            return result?.FirstOrDefault();
        }

        /// <summary>
        /// Lưu Refresh Token mới vào database.
        /// Trước khi lưu, tất cả Refresh Token cũ còn hiệu lực của User sẽ bị thu hồi
        /// để đảm bảo mỗi User chỉ có một Refresh Token hợp lệ tại một thời điểm.
        /// </summary>
        /// <param name="userID">ID của User sở hữu Refresh Token.</param>
        /// <param name="token">Chuỗi Refresh Token cần lưu.</param>
        /// <param name="expiresDate">Thời điểm hết hạn của Refresh Token.</param>
        /// <returns><c>true</c> nếu lưu thành công; <c>false</c> nếu thất bại.</returns>
        public async Task<bool> SaveRefreshTokenAsync(Guid userID, string token, DateTime expiresDate)
        {
            // Thu hồi tất cả Refresh Token cũ còn hiệu lực của User
            var revokeOldSql = @"
                UPDATE refresh_token 
                SET IsRevoked   = 1,
                    RevokedDate = @RevokedDate
                WHERE UserID = @UserID AND IsRevoked = 0";

            await _databaseService.ExecuteUsingCommandText(Constants.MasterDatabaseID, revokeOldSql, new Dictionary<string, object>
            {
                { "@UserID",      userID },
                { "@RevokedDate", DateTime.UtcNow },
            });

            // Chèn Refresh Token mới vào database
            var insertSql = @"
                INSERT INTO refresh_token (RefreshTokenID, UserID, Token, ExpiresDate, IsRevoked, CreatedDate)
                VALUES (@RefreshTokenID, @UserID, @Token, @ExpiresDate, 0, @CreatedDate)";

            var parameters = new Dictionary<string, object>
            {
                { "@RefreshTokenID", Guid.NewGuid() },
                { "@UserID",         userID },
                { "@Token",          token },
                { "@ExpiresDate",    expiresDate },
                { "@CreatedDate",    DateTime.UtcNow },
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterDatabaseID, insertSql, parameters);
        }

        /// <summary>
        /// Thu hồi Refresh Token, thường được gọi khi User đăng xuất
        /// hoặc khi cấp phát Refresh Token mới (token rotation).
        /// </summary>
        /// <param name="token">Chuỗi Refresh Token cần thu hồi.</param>
        /// <returns><c>true</c> nếu thu hồi thành công; <c>false</c> nếu thất bại.</returns>
        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            var sql = @"
                UPDATE refresh_token 
                SET IsRevoked   = 1,
                    RevokedDate = @RevokedDate
                WHERE Token = @Token";

            var parameters = new Dictionary<string, object>
            {
                { "@Token",       token },
                { "@RevokedDate", DateTime.UtcNow },
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterDatabaseID, sql, parameters);
        }

        #endregion

        #region Tenant

        /// <summary>
        /// Lưu thông tin Tenant mới vào database trong một transaction có sẵn.
        /// </summary>
        /// <param name="tenant">Đối tượng Tenant cần lưu.</param>
        /// <param name="cnn">Connection đang mở.</param>
        /// <param name="tran">Transaction đang hoạt động.</param>
        /// <returns><c>true</c> nếu lưu thành công; <c>false</c> nếu thất bại.</returns>
        public async Task<bool> SaveTenantAsync(Tenant tenant, IDbConnection cnn, IDbTransaction tran)
        {
            var sql = @"
                INSERT INTO tenant 
                    (TenantID, TenantCode, TenantName, ContactEmail, ContactPhone,
                     IsActive, ExpiredDate, CreatedDate, IsDeleted)
                VALUES 
                    (@TenantID, @TenantCode, @TenantName, @ContactEmail, @ContactPhone,
                     @IsActive, @ExpiredDate, @CreatedDate, @IsDeleted)";

            var parameters = new Dictionary<string, object>
            {
                { "@TenantID",     tenant.TenantID },
                { "@TenantCode",   tenant.TenantCode },
                { "@TenantName",   tenant.TenantName },
                { "@ContactEmail", tenant.ContactEmail ?? (object)DBNull.Value },
                { "@ContactPhone", tenant.ContactPhone ?? (object)DBNull.Value },
                { "@IsActive",     tenant.IsActive },
                { "@ExpiredDate",  tenant.ExpiredDate ?? (object)DBNull.Value },
                { "@CreatedDate",  tenant.CreatedDate },
                { "@IsDeleted",    tenant.IsDeleted },
            };

            return await _databaseService.ExecuteUsingCommandText(cnn, tran, sql, parameters);
        }

        #endregion

        #region Tenant User

        /// <summary>
        /// Tạo liên kết giữa User và Tenant trong bảng tenant_user trong một transaction có sẵn.
        /// </summary>
        /// <param name="tenantID">ID của Tenant.</param>
        /// <param name="userID">ID của User.</param>
        /// <param name="cnn">Connection đang mở.</param>
        /// <param name="tran">Transaction đang hoạt động.</param>
        /// <returns><c>true</c> nếu lưu thành công; <c>false</c> nếu thất bại.</returns>
        public async Task<bool> SaveTenantUserAsync(Guid tenantID, Guid userID, IDbConnection cnn, IDbTransaction tran)
        {
            var sql = @"
                INSERT INTO tenant_user (TenantUserID, TenantID, UserID, AssignedDate)
                VALUES (@TenantUserID, @TenantID, @UserID, @AssignedDate)";

            var parameters = new Dictionary<string, object>
            {
                { "@TenantUserID", Guid.NewGuid() },
                { "@TenantID",     tenantID },
                { "@UserID",       userID },
                { "@AssignedDate", DateTime.UtcNow },
            };

            return await _databaseService.ExecuteUsingCommandText(cnn, tran, sql, parameters);
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu liên quan đến một lần đăng ký thất bại.
        /// Thứ tự xóa: tenant_user → tenant → user (theo chiều FK).
        /// </summary>
        /// <param name="userID">ID của User cần xóa.</param>
        /// <param name="tenantID">ID của Tenant cần xóa.</param>
        /// <returns><c>true</c> nếu xóa thành công toàn bộ; <c>false</c> nếu có bước thất bại.</returns>
        public async Task<bool> ClearInfoRegisterErrorAsync(Guid userID, Guid tenantID)
        {
            var sql = @"
                DELETE FROM tenant_user WHERE UserID   = @UserID;
                DELETE FROM tenant      WHERE TenantID = @TenantID;
                DELETE FROM user        WHERE UserID   = @UserID;";

            var parameters = new Dictionary<string, object>
            {
                { "@UserID",   userID },
                { "@TenantID", tenantID },
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterDatabaseID, sql, parameters);
        }

        /// <summary>
        /// Cập nhật DatabaseID vào tenant_user sau khi tạo xong database thật.
        /// </summary>
        /// <param name="tenantID">ID của Tenant.</param>
        /// <param name="userID">ID của User.</param>
        /// <param name="databaseID">DatabaseID vừa được tạo.</param>
        /// <returns><c>true</c> nếu cập nhật thành công; <c>false</c> nếu thất bại.</returns>
        public async Task<bool> UpdateTenantUserDatabaseIDAsync(Guid tenantID, Guid userID, Guid databaseID)
        {
            var sql = @"
                UPDATE tenant_user 
                SET DatabaseID = @DatabaseID
                WHERE TenantID = @TenantID AND UserID = @UserID";

            var parameters = new Dictionary<string, object>
            {
                { "@DatabaseID", databaseID },
                { "@TenantID",   tenantID },
                { "@UserID",     userID },
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterDatabaseID, sql, parameters);
        }

        #endregion

        #region Tenant Database

        /// <summary>
        /// Lưu thông tin kết nối database mới vào bảng tenant_database.
        /// </summary>
        /// <param name="tenantDatabase">Đối tượng TenantDatabase cần lưu.</param>
        /// <returns><c>true</c> nếu lưu thành công; <c>false</c> nếu thất bại.</returns>
        public async Task<bool> SaveTenantDatabaseAsync(TenantDatabase tenantDatabase)
        {
            var sql = @"
                INSERT INTO tenant_database 
                    (DatabaseID, TenantID, Server, Port, `Database`, UserID, Password,
                     VersionDB, Status, CreatedDate)
                VALUES 
                    (@DatabaseID, @TenantID, @Server, @Port, @Database, @UserID, @Password,
                     @VersionDB, @Status, @CreatedDate)";

            var parameters = new Dictionary<string, object>
            {
                { "@DatabaseID",  tenantDatabase.DatabaseID },
                { "@TenantID",    tenantDatabase.TenantID },
                { "@Server",      tenantDatabase.Server },
                { "@Port",        tenantDatabase.Port },
                { "@Database",    tenantDatabase.Database },
                { "@UserID",      tenantDatabase.UserID },
                { "@Password",    tenantDatabase.Password },
                { "@VersionDB",   tenantDatabase.VersionDB },
                { "@Status",      tenantDatabase.Status },
                { "@CreatedDate", DateTime.Now },
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterDatabaseID, sql, parameters);
        }

        #endregion
    }
}