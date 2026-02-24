using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using BASE.Service.Core.Services;
using EasyMart.DLBase;

namespace EasyMart.DL.Auth
{
    /// <summary>
    /// Data Layer xử lý các thao tác liên quan đến xác thực (Authentication).
    /// Bao gồm: quản lý thông tin User và Refresh Token trong database.
    /// </summary>
    public class DLAuth : DLBaseEasyMart
    {
        /// <summary>
        /// Khởi tạo <see cref="DLAuth"/> với dịch vụ kết nối MySQL.
        /// </summary>
        /// <param name="databaseService">Dịch vụ thao tác với MySQL database.</param>
        public DLAuth(IMySQLService databaseService) : base(databaseService)
        {
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

        /// <summary>
        /// Lưu Refresh Token mới vào database.
        /// Trước khi lưu, tất cả Refresh Token cũ còn hiệu lực của User sẽ bị thu hồi
        /// để đảm bảo mỗi User chỉ có một Refresh Token hợp lệ tại một thời điểm.
        /// </summary>
        /// <param name="userID">ID của User sở hữu Refresh Token.</param>
        /// <param name="token">Chuỗi Refresh Token cần lưu.</param>
        /// <param name="expiresAt">Thời điểm hết hạn của Refresh Token.</param>
        /// <returns><c>true</c> nếu lưu thành công; <c>false</c> nếu thất bại.</returns>
        public async Task<bool> SaveRefreshTokenAsync(Guid userID, string token, DateTime expiresAt)
        {
            // Thu hồi tất cả Refresh Token cũ còn hiệu lực của User
            var revokeOldSql = @"
                UPDATE refresh_token 
                SET IsRevoked = 1,
                    RevokedAt = @RevokedAt
                WHERE UserID = @UserID AND IsRevoked = 0";

            await _databaseService.ExecuteUsingCommandText(Constants.MasterDatabaseID, revokeOldSql, new Dictionary<string, object>
            {
                { "@UserID",    userID },
                { "@RevokedAt", DateTime.UtcNow },
            });

            // Chèn Refresh Token mới vào database
            var insertSql = @"
                INSERT INTO refresh_token (RefreshTokenID, UserID, Token, ExpiresAt, IsRevoked, CreatedAt)
                VALUES (@RefreshTokenID, @UserID, @Token, @ExpiresAt, 0, @CreatedAt)";

            var parameters = new Dictionary<string, object>
            {
                { "@RefreshTokenID", Guid.NewGuid() },
                { "@UserID",         userID },
                { "@Token",          token },
                { "@ExpiresAt",      expiresAt },
                { "@CreatedAt",      DateTime.UtcNow },
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterDatabaseID, insertSql, parameters);
        }

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
        /// Thu hồi Refresh Token, thường được gọi khi User đăng xuất
        /// hoặc khi cấp phát Refresh Token mới (token rotation).
        /// </summary>
        /// <param name="token">Chuỗi Refresh Token cần thu hồi.</param>
        /// <returns><c>true</c> nếu thu hồi thành công; <c>false</c> nếu thất bại.</returns>
        public async Task<bool> RevokeRefreshTokenAsync(string token)
        {
            var sql = @"
                UPDATE refresh_token 
                SET IsRevoked = 1,
                    RevokedAt = @RevokedAt
                WHERE Token = @Token";

            var parameters = new Dictionary<string, object>
            {
                { "@Token",     token },
                { "@RevokedAt", DateTime.UtcNow },
            };

            return await _databaseService.ExecuteUsingCommandText(Constants.MasterDatabaseID, sql, parameters);
        }
    }
}
