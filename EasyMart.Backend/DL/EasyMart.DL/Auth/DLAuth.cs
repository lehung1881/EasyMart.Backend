using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using BASE.Service.Core.Services;
using EasyMart.DLBase;
using System.Data;

namespace EasyMart.DL.Auth
{
    /// <summary>
    /// Data Layer xử lý các thao tác liên quan đến xác thực (Authentication) và định danh Đơn vị.
    /// Bao gồm: quản lý thông tin User, User Refresh Token, EasyMart, EasyMartAssignment và EasyMartDbConfig.
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

            var result = await _databaseService.QueryUsingCommandText<User>(Constants.MasterEasyMartID, sql, parameters);
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

            var result = await _databaseService.QueryUsingCommandText<User>(Constants.MasterEasyMartID, sql, parameters);
            return result?.FirstOrDefault();
        }

        /// <summary>
        /// Lấy thông tin <see cref="UserInfo"/> theo <paramref name="userID"/>
        /// bằng cách kết hợp bảng <c>user</c>, <c>user_easymart_assignment</c> và <c>easymart</c>.
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
                    u.Email,
                    u.FullName,
                    u.AvatarUrl,
                    u.PhoneNumber,
                    em.EasyMartID,
                    em.EasyMartCode,
                    em.EasyMartName
                FROM user u
                LEFT JOIN user_easymart_assignment uema ON u.UserID = uema.UserID
                LEFT JOIN easymart em ON uema.EasyMartID = em.EasyMartID
                WHERE u.UserID = @UserID
                  AND u.IsDeleted = 0
                LIMIT 1";

            var parameters = new Dictionary<string, object>
            {
                { "@UserID", userID }
            };

            var result = await _databaseService.QueryUsingCommandText<UserInfo>(Constants.MasterEasyMartID, sql, parameters);
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
                     IsActive, IsLocked, FailedLoginCount, IsEmailVerified, IsDeleted, CreatedDate)
                VALUES
                    (@UserID, @Email, @FullName, @PasswordHash, @PhoneNumber,
                     @IsActive, @IsLocked, @FailedLoginCount, @IsEmailVerified, @IsDeleted, @CreatedDate)";

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
                { "@CreatedDate",      DateTime.Now }
            };

            return await _databaseService.ExecuteUsingCommandText(cnn, tran, sql, parameters);
        }

        /// <summary>
        /// Cập nhật thông tin xác thực bảo mật của User.
        /// </summary>
        /// <param name="user">Đối tượng User chứa thông tin cần cập nhật.</param>
        /// <returns><c>true</c> nếu cập nhật thành công; <c>false</c> nếu thất bại.</returns>
        public async Task<bool> UpdateAsync(User user)
        {
            var sql = @"
                UPDATE user SET
                    FailedLoginCount = @FailedLoginCount,
                    IsLocked         = @IsLocked,
                    IsActive         = @IsActive,
                    ModifiedDate     = @ModifiedDate
                WHERE UserID = @UserID";

            var parameters = new Dictionary<string, object>
            {
                { "@UserID",           user.UserID },
                { "@FailedLoginCount", user.FailedLoginCount },
                { "@IsLocked",         user.IsLocked },
                { "@IsActive",         user.IsActive },
                { "@ModifiedDate",     DateTime.Now }
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterEasyMartID, sql, parameters);
        }

        #endregion

        #region Refresh Token

        /// <summary>
        /// Lấy thông tin Refresh Token hoạt động từ database để xác thực.
        /// </summary>
        /// <param name="token">Chuỗi Refresh Token cần tìm kiếm.</param>
        /// <returns>Đối tượng <see cref="UserRefreshToken"/> nếu hợp lệ; ngược lại trả về <c>null</c>.</returns>
        public async Task<UserRefreshToken> GetRefreshTokenAsync(string token)
        {
            var sql = @"
                SELECT * FROM user_refresh_token 
                WHERE Token = @Token AND IsRevoked = 0 
                LIMIT 1";

            var parameters = new Dictionary<string, object>
            {
                { "@Token", token }
            };

            var result = await _databaseService.QueryUsingCommandText<UserRefreshToken>(Constants.MasterEasyMartID, sql, parameters);
            return result?.FirstOrDefault();
        }

        /// <summary>
        /// Kiểm tra Refresh Token còn hiệu lực và chưa hết hạn.
        /// </summary>
        /// <param name="token">Chuỗi Refresh Token cần kiểm tra.</param>
        /// <returns>Đối tượng <see cref="UserRefreshToken"/> nếu hợp lệ; ngược lại trả về <c>null</c>.</returns>
        public async Task<UserRefreshToken> GetValidRefreshTokenAsync(string token)
        {
            var sql = @"
                SELECT * FROM user_refresh_token 
                WHERE Token = @Token 
                  AND IsRevoked = 0 
                  AND ExpiresDate > @Now
                LIMIT 1";

            var parameters = new Dictionary<string, object>
            {
                { "@Token", token },
                { "@Now",   DateTime.Now },
            };

            var result = await _databaseService.QueryUsingCommandText<UserRefreshToken>(Constants.MasterEasyMartID, sql, parameters);
            return result?.FirstOrDefault();
        }

        /// <summary>
        /// Lưu Refresh Token mới. Đồng thời thu hồi tất cả các token cũ của User đó.
        /// </summary>
        public async Task<bool> SaveRefreshTokenAsync(Guid userID, string token, DateTime expiresDate)
        {
            // Thu hồi tất cả Refresh Token cũ đang kích hoạt của User này
            var revokeOldSql = @"
                UPDATE user_refresh_token 
                SET IsRevoked   = 1,
                    RevokedDate = @RevokedDate
                WHERE UserID = @UserID AND IsRevoked = 0";

            await _databaseService.ExecuteUsingCommandText(Constants.MasterEasyMartID, revokeOldSql, new Dictionary<string, object>
            {
                { "@UserID",      userID },
                { "@RevokedDate", DateTime.Now },
            });

            // Chèn mã Refresh Token mới
            var insertSql = @"
                INSERT INTO user_refresh_token (RefreshTokenID, UserID, Token, ExpiresDate, IsRevoked, CreatedDate)
                VALUES (@RefreshTokenID, @UserID, @Token, @ExpiresDate, 0, @CreatedDate)";

            var parameters = new Dictionary<string, object>
            {
                { "@RefreshTokenID", Guid.NewGuid() },
                { "@UserID",         userID },
                { "@Token",          token },
                { "@ExpiresDate",    expiresDate },
                { "@CreatedDate",    DateTime.Now },
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterEasyMartID, insertSql, parameters);
        }

        /// <summary>
        /// Thu hồi Token cụ thể khi Logout hoặc áp dụng quy trình xoay vòng token.
        /// </summary>
        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            var sql = @"
                UPDATE user_refresh_token 
                SET IsRevoked   = 1,
                    RevokedDate = @RevokedDate
                WHERE Token = @Token";

            var parameters = new Dictionary<string, object>
            {
                { "@Token",       token },
                { "@RevokedDate", DateTime.Now },
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterEasyMartID, sql, parameters);
        }

        #endregion

        #region EasyMart (EasyMart)

        /// <summary>
        /// Lưu thông tin Siêu thị/Đơn vị mới vào database trong một transaction có sẵn.
        /// </summary>
        public async Task<bool> SaveEasyMartAsync(EasyMartEntity easyMart, IDbConnection cnn, IDbTransaction tran)
        {
            var sql = @"
                INSERT INTO easymart 
                    (EasyMartID, EasyMartCode, EasyMartName, ContactEmail, ContactPhone,
                     IsActive, ExpiredDate, CreatedDate, IsDeleted)
                VALUES 
                    (@EasyMartID, @EasyMartCode, @EasyMartName, @ContactEmail, @ContactPhone,
                     @IsActive, @ExpiredDate, @CreatedDate, @IsDeleted)";

            var parameters = new Dictionary<string, object>
            {
                { "@EasyMartID",   easyMart.EasyMartID },
                { "@EasyMartCode", easyMart.EasyMartCode },
                { "@EasyMartName", easyMart.EasyMartName },
                { "@ContactEmail", easyMart.ContactEmail ?? (object)DBNull.Value },
                { "@ContactPhone", easyMart.ContactPhone ?? (object)DBNull.Value },
                { "@IsActive",     easyMart.IsActive },
                { "@ExpiredDate",  easyMart.ExpiredDate ?? (object)DBNull.Value },
                { "@CreatedDate",  easyMart.CreatedDate },
                { "@IsDeleted",    easyMart.IsDeleted },
            };

            return await _databaseService.ExecuteUsingCommandText(cnn, tran, sql, parameters);
        }

        #endregion

        #region EasyMart Assignment (EasyMart User)

        /// <summary>
        /// Tạo liên kết phân công giữa User và Siêu thị (Mối quan hệ N-N) trong một transaction có sẵn.
        /// </summary>
        public async Task<bool> SaveEasyMartAssignmentAsync(Guid easymartID, Guid userID, IDbConnection cnn, IDbTransaction tran)
        {
            var sql = @"
                INSERT INTO user_easymart_assignment (EasyMartID, UserID, AssignedDate)
                VALUES (@EasyMartID, @UserID, @AssignedDate)";

            var parameters = new Dictionary<string, object>
            {
                { "@EasyMartID",   easymartID },
                { "@UserID",       userID },
                { "@AssignedDate", DateTime.Now },
            };

            return await _databaseService.ExecuteUsingCommandText(cnn, tran, sql, parameters);
        }

        /// <summary>
        /// Xóa toàn bộ dữ liệu liên quan đến một lần đăng ký bị lỗi để dọn dẹp hệ thống.
        /// Xóa theo thứ tự đảm bảo toàn vẹn: u_em_assignment → easymart → user.
        /// </summary>
        public async Task<bool> ClearInfoRegisterErrorAsync(Guid userID, Guid easymartID)
        {
            var sql = @"
                DELETE FROM user_easymart_assignment WHERE UserID   = @UserID;
                DELETE FROM easymart                  WHERE EasyMartID = @EasyMartID;
                DELETE FROM user                       WHERE UserID   = @UserID;";

            var parameters = new Dictionary<string, object>
            {
                { "@UserID",     userID },
                { "@EasyMartID", easymartID },
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterEasyMartID, sql, parameters);
        }

        #endregion

        #region EasyMart DB Config (EasyMart Database)

        /// <summary>
        /// Lưu cấu hình thông tin kết nối database riêng độc lập của một Siêu thị.
        /// Lưu ý: Cột DbConfigID là trường AUTO_INCREMENT nên không cần đưa vào câu INSERT.
        /// </summary>
        public async Task<bool> SaveEasyMartDbConfigAsync(EasyMartDbConfig dbConfig)
        {
            var sql = @"
                INSERT INTO easymart_db_config 
                    (EasyMartID, Server, Port, `Database`, UserID, Password, VersionDB, Status, CreatedDate)
                VALUES 
                    (@EasyMartID, @Server, @Port, @Database, @UserID, @Password, @VersionDB, @Status, @CreatedDate)";

            var parameters = new Dictionary<string, object>
            {
                { "@EasyMartID",   dbConfig.EasyMartID },
                { "@Server",       dbConfig.Server },
                { "@Port",         dbConfig.Port },
                { "@Database",     dbConfig.Database },
                { "@UserID",       dbConfig.UserID },
                { "@Password",     dbConfig.Password },
                { "@VersionDB",    dbConfig.VersionDB },
                { "@Status",       dbConfig.Status },
                { "@CreatedDate",  DateTime.Now },
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterEasyMartID, sql, parameters);
        }

        #endregion
    }
}