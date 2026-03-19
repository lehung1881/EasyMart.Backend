using BASE.Service.Core.BL;
using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using BASE.Service.Core.Utils;
using EasyMart.BLBase;
using EasyMart.DL.Auth;
using MySql.Data.MySqlClient;

namespace EasyMart.BL.Auth
{
    public class BLAuth : BLBaseEasyMart<DLAuth>
    {
        private readonly JwtHelper _jwtHelper;

        public BLAuth(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
            _jwtHelper = new JwtHelper();
        }

        public override DLAuth CreateDL() => new DLAuth(_mySQLService);

        #region Public API

        /// <summary>
        /// Đăng nhập.
        /// Xác minh tài khoản, mật khẩu và trả về Access Token + Refresh Token.
        /// </summary>
        /// <param name="request">Thông tin đăng nhập từ client.</param>
        /// <returns>
        /// <see cref="ServiceResponse"/> với <c>Data</c> là <see cref="LoginResponse"/> nếu thành công.
        /// </returns>
        public async Task<ServiceResponse> LoginAsync(LoginRequest request)
        {
            var res = new ServiceResponse();

            // Bước 1: Kiểm tra tài khoản tồn tại
            var user = await DLObject.GetByEmailAsync(request.Email);
            if (user is null)
            {
                res.OnError("Tài khoản hoặc mật khẩu không chính xác");
                return res;
            }

            // Bước 2: Kiểm tra tài khoản có bị khóa không
            if (user.IsLocked)
            {
                res.OnError("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên");
                return res;
            }

            // Bước 3: Kiểm tra tài khoản đã được kích hoạt chưa
            if (!user.IsActive)
            {
                res.OnError("Tài khoản chưa được kích hoạt. Vui lòng kiểm tra email");
                return res;
            }

            // Bước 4: Xác minh mật khẩu bằng BCrypt
            bool isPasswordValid = CommonFn.VerifyPassword(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                await HandleFailedLoginAsync(user);
                res.OnError("Tài khoản hoặc mật khẩu không chính xác");
                return res;
            }

            // Bước 5: Reset số lần đăng nhập sai nếu đăng nhập thành công
            await ResetFailedLoginAsync(user);

            // Bước 6: Tạo JWT Token
            var userInfo = await DLObject.GetUserInfoByIDAsync(user.UserID);
            var accessToken = _jwtHelper.GenerateAccessToken(userInfo);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            // Bước 7: Lưu Refresh Token vào DB
            var refreshTokenExpires = GlobalConfig.AppSettings.JwtSettings.RefreshTokenExpires;
            await DLObject.SaveRefreshTokenAsync(user.UserID, refreshToken, DateTime.Now.AddSeconds(refreshTokenExpires));

            res.OnSuccess(new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresDate = DateTime.Now.AddSeconds(GlobalConfig.AppSettings.JwtSettings.AccessTokenExpires),
                UserInfo = userInfo,
            });

            return res;
        }

        /// <summary>
        /// Làm mới Access Token bằng Refresh Token hợp lệ.
        /// Áp dụng cơ chế Token Rotation: Refresh Token cũ bị thu hồi,
        /// một Refresh Token mới được cấp phát cùng lúc với Access Token mới.
        /// </summary>
        /// <param name="request">Request chứa Refresh Token hiện tại.</param>
        /// <returns>Access Token mới, Refresh Token mới và thông tin User.</returns>
        public async Task<ServiceResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var res = new ServiceResponse();

            // Bước 1: Kiểm tra Refresh Token có hợp lệ không
            var existingToken = await DLObject.GetValidRefreshTokenAsync(request.RefreshToken);
            if (existingToken is null)
            {
                res.OnError(ServiceResponseCode.TokenExpired, "Refresh Token không hợp lệ hoặc đã hết hạn");
                return res;
            }

            // Bước 2: Lấy thông tin User từ token
            var userInfo = await DLObject.GetUserInfoByIDAsync(existingToken.UserID);
            if (userInfo is null)
            {
                res.OnError(ServiceResponseCode.NotFound, "Không tìm thấy thông tin người dùng");
                return res;
            }

            // Bước 3: Cấp Access Token mới + Refresh Token mới (Token Rotation)
            var newAccessToken = _jwtHelper.GenerateAccessToken(userInfo);
            var newRefreshToken = _jwtHelper.GenerateRefreshToken();

            // Bước 4: Thu hồi token cũ, lưu token mới
            var refreshTokenExpires = GlobalConfig.AppSettings.JwtSettings.RefreshTokenExpires;
            await DLObject.SaveRefreshTokenAsync(existingToken.UserID, newRefreshToken, DateTime.Now.AddSeconds(refreshTokenExpires));

            res.OnSuccess(new LoginResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                UserInfo = userInfo,
                ExpiresDate = DateTime.Now.AddSeconds(GlobalConfig.AppSettings.JwtSettings.AccessTokenExpires),
            });

            return res;
        }

        /// <summary>
        /// Đăng ký tài khoản người dùng mới.
        /// Kiểm tra email trùng lặp, mã hóa mật khẩu BCrypt, tạo Tenant và liên kết TenantUser
        /// trong một transaction, sau đó tạo database riêng cho Tenant từ template DB.
        /// </summary>
        /// <param name="request">Thông tin đăng ký từ client.</param>
        /// <returns>
        /// <see cref="ServiceResponse"/> với <c>Data</c> là <see cref="UserInfo"/> nếu thành công.
        /// </returns>
        public async Task<ServiceResponse> RegisterAsync(RegisterRequest request)
        {
            var res = new ServiceResponse();

            // Bước 1: Validate dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                res.OnError(ServiceResponseCode.InvalidData, "Email không được để trống");
                return res;
            }

            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                res.OnError(ServiceResponseCode.InvalidData, "Họ và tên không được để trống");
                return res;
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                res.OnError(ServiceResponseCode.InvalidData, "Mật khẩu không được để trống");
                return res;
            }

            // Bước 2: Kiểm tra email đã tồn tại chưa
            var isEmailExists = await DLObject.IsEmailExistsAsync(request.Email);
            if (isEmailExists)
            {
                res.OnError(ServiceResponseCode.Duplicate, "Email đã được sử dụng. Vui lòng chọn email khác");
                return res;
            }

            // Bước 3: Chuẩn bị dữ liệu User và Tenant
            var newUser = new User
            {
                UserID = Guid.NewGuid(),
                Email = request.Email.Trim().ToLower(),
                FullName = request.FullName.Trim(),
                PasswordHash = CommonFn.HashPassword(request.Password),
                PhoneNumber = request.PhoneNumber?.Trim(),
                IsActive = true,
                IsLocked = false,
                FailedLoginCount = 0,
                IsEmailVerified = false,
                IsDeleted = false,
            };

            var newTenant = new Tenant
            {
                TenantID = Guid.NewGuid(),
                TenantCode = GenerateTenantCode(),
                TenantName = request.FullName.Trim(),
                ContactEmail = request.Email.Trim().ToLower(),
                ContactPhone = request.PhoneNumber?.Trim(),
                IsActive = true,
                ExpiredDate = DateTime.Now.AddDays(7),
                CreatedDate = DateTime.Now,
                IsDeleted = false,
            };

            // Bước 4: Lưu User + Tenant + TenantUser trong một transaction
            var cnn = await _mySQLService.GetDBConnectionAsync(Constants.MasterDatabaseID);
            using (cnn)
            {
                cnn.Open();
                using var transaction = cnn.BeginTransaction();
                try
                {
                    var saveUserResult = await DLObject.SaveUserAsync(newUser, cnn, transaction);
                    if (!saveUserResult)
                    {
                        transaction.Rollback();
                        res.OnError(ServiceResponseCode.Exception, "Đăng ký thất bại. Vui lòng thử lại");
                        return res;
                    }

                    var saveTenantResult = await DLObject.SaveTenantAsync(newTenant, cnn, transaction);
                    if (!saveTenantResult)
                    {
                        transaction.Rollback();
                        res.OnError(ServiceResponseCode.Exception, "Khởi tạo cửa hàng thất bại. Vui lòng thử lại");
                        return res;
                    }

                    var saveTenantUserResult = await DLObject.SaveTenantUserAsync(
                        newTenant.TenantID,
                        newUser.UserID,
                        cnn,
                        transaction
                    );

                    if (!saveTenantUserResult)
                    {
                        transaction.Rollback();
                        res.OnError(ServiceResponseCode.Exception, "Liên kết tài khoản với cửa hàng thất bại. Vui lòng thử lại");
                        return res;
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            // Bước 5: Tạo database riêng cho Tenant từ template DB
            var newDatabaseID = await CreateTenantDatabaseAsync(newTenant.TenantID, newTenant.TenantCode);
            if (newDatabaseID is null)
            {
                await ClearInfoRegisterErrorAsync(newUser.UserID, newTenant.TenantID);
                res.OnError(ServiceResponseCode.Exception, "Tạo database cửa hàng thất bại. Vui lòng thử lại");
                return res;
            }

            // Bước 6: Cập nhật DatabaseID thật vào tenant_user
            await DLObject.UpdateTenantUserDatabaseIDAsync(
                newTenant.TenantID,
                newUser.UserID,
                newDatabaseID.Value
            );

            // Bước 7: Trả về thông tin User vừa tạo
            var userInfo = await DLObject.GetUserInfoByIDAsync(newUser.UserID);
            res.OnSuccess(userInfo);

            return res;
        }

        #endregion

        #region Tenant Database

        /// <summary>
        /// Tạo database mới cho Tenant từ template DB,
        /// sau đó lưu thông tin kết nối vào bảng tenant_database.
        /// Nếu lưu metadata thất bại, database vừa tạo sẽ bị DROP để tránh dữ liệu rác.
        /// </summary>
        /// <param name="tenantID">ID của Tenant vừa tạo.</param>
        /// <param name="tenantCode">Mã Tenant, dùng để đặt tên database mới.</param>
        /// <returns><see cref="Guid"/> DatabaseID nếu thành công; <c>null</c> nếu thất bại.</returns>
        private async Task<Guid?> CreateTenantDatabaseAsync(Guid tenantID, string tenantCode)
        {
            var templateConnStr = GlobalConfig.AppSettings.ConnectionStrings.TemplateDB;
            var masterConnStr = GlobalConfig.AppSettings.ConnectionStrings.MasterDB;

            var newDatabaseName = $"easymart_{tenantCode}_{DateTime.Now.Year}";
            var tempDir = Path.Combine(AppContext.BaseDirectory, "temp");
            var backupFile = Path.Combine(tempDir, $"{newDatabaseName}_{Guid.NewGuid():N}.sql");
            var dbCreated = false;

            try
            {
                // Bước 1: Tạo thư mục temp nếu chưa có
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                // Bước 2: Backup DB template ra file .sql
                using (var conn = new MySqlConnection(templateConnStr))
                {
                    await conn.OpenAsync();
                    using var cmd = new MySqlCommand { Connection = conn };
                    using var backup = new MySqlBackup(cmd);
                    backup.ExportInfo.ResetAutoIncrement = true;
                    backup.ExportInfo.ExportRoutinesWithoutDefiner = true;
                    backup.ExportInfo.ExportTableStructure = true;
                    backup.ExportInfo.ExportRows = true;
                    backup.ExportToFile(backupFile);
                }

                // Bước 3: Tạo database mới + restore từ file backup
                using (var conn = new MySqlConnection(masterConnStr))
                {
                    await conn.OpenAsync();

                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"CREATE DATABASE IF NOT EXISTS `{newDatabaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_as_ci;";
                        await cmd.ExecuteNonQueryAsync();
                    }

                    dbCreated = true;

                    conn.ChangeDatabase(newDatabaseName);

                    using var restoreCmd = new MySqlCommand { Connection = conn };
                    using var restore = new MySqlBackup(restoreCmd);
                    restore.ImportInfo.IgnoreSqlError = false;
                    restore.ImportFromFile(backupFile);
                }

                // Bước 4: Lưu thông tin kết nối vào tenant_database
                var builder = new MySqlConnectionStringBuilder(masterConnStr);
                var databaseID = Guid.NewGuid();

                var tenantDatabase = new TenantDatabase
                {
                    DatabaseID = databaseID,
                    TenantID = tenantID,
                    Server = builder.Server,
                    Port = (int)builder.Port,
                    Database = newDatabaseName,
                    UserID = builder.UserID,
                    Password = builder.Password,
                    VersionDB = "0.0.0.1",
                    Status = 0,
                    CreatedDate = DateTime.Now,
                };

                var saveResult = await DLObject.SaveTenantDatabaseAsync(tenantDatabase);
                if (!saveResult)
                {
                    // Lưu metadata thất bại → DROP DATABASE để tránh DB rác
                    await DropDatabaseAsync(masterConnStr, newDatabaseName);
                    return null;
                }

                return databaseID;
            }
            catch (Exception ex)
            {
                // DB đã tạo nhưng restore hoặc lưu metadata thất bại → DROP DATABASE
                if (dbCreated)
                    await DropDatabaseAsync(masterConnStr, newDatabaseName);

                throw new Exception($"Tạo database cho Tenant [{tenantCode}] thất bại: {ex.Message}", ex);
            }
            finally
            {
                // Bước 5: Xóa file backup tạm dù thành công hay thất bại
                if (File.Exists(backupFile))
                    File.Delete(backupFile);
            }
        }

        /// <summary>
        /// Xóa database nếu quá trình tạo bị lỗi giữa chừng.
        /// </summary>
        private static async Task DropDatabaseAsync(string connStr, string databaseName)
        {
            try
            {
                using var conn = new MySqlConnection(connStr);
                await conn.OpenAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = $"DROP DATABASE IF EXISTS `{databaseName}`;";
                await cmd.ExecuteNonQueryAsync();
            }
            catch
            {
                // Bỏ qua lỗi DROP — không throw để tránh che mất exception gốc
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Xử lý khi đăng nhập sai: tăng counter, khóa tài khoản nếu vượt ngưỡng 5 lần.
        /// </summary>
        private async Task HandleFailedLoginAsync(User user)
        {
            user.FailedLoginCount++;

            // Khóa tài khoản sau 5 lần sai liên tiếp
            if (user.FailedLoginCount >= 5)
            {
                user.IsLocked = true;
            }

            await DLObject.UpdateAsync(user);
        }

        /// <summary>
        /// Reset counter đăng nhập sai khi đăng nhập thành công.
        /// </summary>
        private async Task ResetFailedLoginAsync(User user)
        {
            if (user.FailedLoginCount > 0)
            {
                user.FailedLoginCount = 0;
                await DLObject.UpdateAsync(user);
            }
        }

        /// <summary>
        /// Rollback thủ công khi tạo database thất bại sau khi đã commit user/tenant/tenant_user.
        /// Ủy thác việc xóa xuống DL để đảm bảo đúng thứ tự FK.
        /// </summary>
        private async Task ClearInfoRegisterErrorAsync(Guid userID, Guid tenantID)
        {
            try
            {
                await DLObject.ClearInfoRegisterErrorAsync(userID, tenantID);
            }
            catch (Exception ex)
            {
                // Không throw — không để lỗi rollback che mất lỗi gốc
                // TODO: ghi log cảnh báo để xử lý dữ liệu rác thủ công nếu cần
                _ = ex;
            }
        }

        /// <summary>
        /// Sinh TenantCode ngẫu nhiên theo định dạng SHOP_xxxxxxxx.
        /// </summary>
        private static string GenerateTenantCode()
        {
            return Guid.NewGuid().ToString("N")[..8].ToLower();
        }

        /// <summary>
        /// Lấy thông tin người dùng hiện tại theo UserID.
        /// Dùng cho endpoint /me để kiểm tra phiên đăng nhập còn hợp lệ không.
        /// </summary>
        /// <param name="userID">ID của người dùng cần lấy thông tin.</param>
        /// <returns>
        /// <see cref="ServiceResponse"/> với <c>Data</c> là <see cref="UserInfo"/> nếu thành công.
        /// </returns>
        public async Task<ServiceResponse> GetUserInfoAsync(Guid userID)
        {
            var res = new ServiceResponse();

            var userInfo = await DLObject.GetUserInfoByIDAsync(userID);
            if (userInfo is null)
            {
                res.OnError(ServiceResponseCode.NotFound, "Không tìm thấy thông tin người dùng");
                return res;
            }

            res.OnSuccess(userInfo);
            return res;
        }

        #endregion
    }
}