using BASE.Service.Core.BL;
using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using BASE.Service.Core.Utils;
using EasyMart.BLBase;
using EasyMart.Constant.Constant;
using EasyMart.DL.Auth;
using EasyMart.Model.System;
using MySql.Data.MySqlClient;
using System.Data;

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
        /// Kiểm tra email trùng lặp, mã hóa mật khẩu BCrypt, tạo EasyMart và liên kết EasyMartAssignment
        /// trong một transaction, sau đó tạo database riêng cho Đơn vị từ template DB.
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

            // Bước 3: Chuẩn bị dữ liệu User và EasyMart (Chỉ định rõ Namespace để tránh trùng lặp dự án)
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

            var newEasyMart = new EasyMartEntity
            {
                EasyMartID = Guid.NewGuid(),
                EasyMartCode = GenerateEasyMartCode(),
                EasyMartName = request.EasyMartName,
                ContactEmail = request.Email.Trim().ToLower(),
                ContactPhone = request.PhoneNumber?.Trim(),
                IsActive = 1,
                ExpiredDate = DateTime.Now.AddDays(7),
                CreatedDate = DateTime.Now,
                IsDeleted = 0,
            };

            // Bước 4: Lưu User + EasyMart + EasyMartAssignment trong một transaction liên kết thương mại
            var cnn = await _mySQLService.GetDBConnectionAsync(Constants.MasterEasyMartID);
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

                    var saveEasyMartResult = await DLObject.SaveEasyMartAsync(newEasyMart, cnn, transaction);
                    if (!saveEasyMartResult)
                    {
                        transaction.Rollback();
                        res.OnError(ServiceResponseCode.Exception, "Khởi tạo cửa hàng thất bại. Vui lòng thử lại");
                        return res;
                    }

                    var saveAssignmentResult = await DLObject.SaveEasyMartAssignmentAsync(
                        newEasyMart.EasyMartID,
                        newUser.UserID,
                        cnn,
                        transaction
                    );

                    if (!saveAssignmentResult)
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

            // Bước 5: Tạo database độc lập riêng cho Đơn vị mới từ template DB
            var newDatabaseSuccess = await CreateEasyMartDatabaseAsync(newEasyMart.EasyMartID, newEasyMart.EasyMartCode);
            if (!newDatabaseSuccess)
            {
                await ClearInfoRegisterErrorAsync(newUser.UserID, newEasyMart.EasyMartID);
                res.OnError(ServiceResponseCode.Exception, "Tạo database cửa hàng thất bại. Vui lòng thử lại");
                return res;
            }

            // Bước 6: Trả về thông tin User vừa tạo thành công
            var userInfo = await DLObject.GetUserInfoByIDAsync(newUser.UserID);

            // Bước 7: Đồng bộ User về dữ liệu vừa tạo
            await SyncUserToCompanySystemAsync(userInfo);

            res.OnSuccess(userInfo);

            return res;
        }

        /// <summary>
        /// Xử lý ngầm định User sau khi đăng ký và set quyền ngầm định
        /// </summary>
        /// <param name="newUser"></param>
        /// <param name="easyMartID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task SyncUserToCompanySystemAsync(UserInfo newUser)
        {
            try
            {
                var sysMscUser = new SysMscUser
                {
                    UserID = newUser.UserID,
                    Email = newUser.Email,
                    FullName = newUser.FullName,
                    MobilePhone = newUser.PhoneNumber,
                    Status = 0,
                    RoleID = Guid.Parse(EasyMartConstant.RoleAdminID),
                    RoleCode = EasyMartConstant.RoleAdminCode,
                    RoleName = EasyMartConstant.RoleAdminName,
                    IsSystem = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = "Hệ thống tự đồng bộ",
                    ModifiedDate = DateTime.Now,
                    ModifiedBy = "Hệ thống tự đồng bộ",
                    ModelState = ModelState.Insert
                };
                SetEasyMartID(newUser.EasyMartID);
                await SaveDataAsync(sysMscUser);
            }
            catch (Exception ex)
            {
                throw new Exception($"[SyncUserToCompanySystemAsync] Đồng bộ tài khoản vào hệ thống công ty thất bại. UserID: {newUser.UserID}", ex);
            }
        }

        #endregion

        #region EasyMart Database Provisioning

        /// <summary>
        /// Tạo database mới cho Đơn vị từ template DB, sau đó lưu metadata vào bảng easymart_db_config.
        /// Nếu lưu cấu hình metadata thất bại, database vừa tạo sẽ bị DROP để tránh sinh rác dữ liệu.
        /// </summary>
        /// <param name="easymartID">ID của Đơn vị vừa tạo.</param>
        /// <param name="easyMartCode">Mã định danh Đơn vị, dùng để đặt tên DB vật lý.</param>
        /// <returns><c>true</c> nếu toàn bộ quy trình thành công; <c>false</c> nếu thất bại.</returns>
        private async Task<bool> CreateEasyMartDatabaseAsync(Guid easymartID, string easyMartCode)
        {
            var templateConnStr = GlobalConfig.AppSettings.ConnectionStrings.TemplateDB;
            var masterConnStr = GlobalConfig.AppSettings.ConnectionStrings.MasterDB;

            var newDatabaseName = $"easymart_{easyMartCode}_{DateTime.Now.Year}";
            var tempDir = Path.Combine(AppContext.BaseDirectory, "temp");
            var backupFile = Path.Combine(tempDir, $"{newDatabaseName}_{Guid.NewGuid():N}.sql");
            var dbCreated = false;

            try
            {
                // Bước 1: Tạo thư mục tạm lưu file cấu trúc kết xuất nếu chưa tồn tại
                if (!Directory.Exists(tempDir))
                    Directory.CreateDirectory(tempDir);

                // Bước 2: Sao lưu cơ sở dữ liệu mẫu (Template DB) ra tệp cấu trúc script .sql
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

                // Bước 3: Khởi tạo schema Database mới vật lý và phục hồi cấu trúc dữ liệu
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

                // Bước 4: Đóng gói dữ liệu bản ghi ánh xạ lưu thông tin kết nối vào easymart_db_config
                var builder = new MySqlConnectionStringBuilder(masterConnStr);

                var dbConfig = new EasyMartDbConfig
                {
                    EasyMartID = easymartID,
                    Server = builder.Server,
                    Port = (int)builder.Port,
                    Database = newDatabaseName,
                    UserID = builder.UserID,
                    Password = builder.Password,
                    VersionDB = "1.0.0.0",
                    Env = "g2",
                    Status = 0,
                    CreatedDate = DateTime.Now
                };

                // Gọi hàm lưu thông tin cấu hình xuống DL
                var saveResult = await DLObject.SaveEasyMartDbConfigAsync(dbConfig);
                if (!saveResult)
                {
                    // Lưu cấu hình metadata lỗi -> Hủy bỏ Database vật lý vừa tạo để tránh mâu thuẫn hệ thống
                    await DropDatabaseAsync(masterConnStr, newDatabaseName);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                // Nếu DB schema vật lý đã được dựng nhưng lỗi phát sinh ở bước sau -> tiến hành DROP DB cô lập lỗi
                if (dbCreated)
                    await DropDatabaseAsync(masterConnStr, newDatabaseName);

                throw new Exception($"Tạo dữ liệu Database cho Siêu thị mã [{easyMartCode}] gặp sự cố: {ex.Message}", ex);
            }
            finally
            {
                // Bước 5: Thực hiện dọn dẹp giải phóng file lưu trữ script .sql tạm thời
                if (File.Exists(backupFile))
                    File.Delete(backupFile);
            }
        }

        /// <summary>
        /// Xóa database biệt lập khi quá trình đồng bộ hóa dữ liệu phát sinh lỗi nghiêm trọng giữa chừng.
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
                // Nuốt biệt lệ xử lý DROP âm thầm để không đè mất stack trace lỗi nghiệp vụ chính ở khối catch ngoại vi
            }
        }

        #endregion

        #region Private Helpers

        /// <summary>
        /// Xử lý tăng số lần đăng nhập lỗi liên tục, tự động khóa tài khoản khi chạm ngưỡng quy định.
        /// </summary>
        private async Task HandleFailedLoginAsync(User user)
        {
            user.FailedLoginCount++;

            if (user.FailedLoginCount >= 5)
            {
                user.IsLocked = true;
            }

            await DLObject.UpdateAsync(user);
        }

        /// <summary>
        /// Khôi phục trạng thái bộ đếm đăng nhập sai khi người dùng xác thực thành công.
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
        /// Dọn dẹp bản ghi thông tin đăng ký lỗi do quá trình khởi tạo Database cô lập thất bại.
        /// </summary>
        private async Task ClearInfoRegisterErrorAsync(Guid userID, Guid easymartID)
        {
            try
            {
                await DLObject.ClearInfoRegisterErrorAsync(userID, easymartID);
            }
            catch (Exception ex)
            {
                // Ghi nhận log thầm lặng, không throw lỗi ra ngoài tránh phá hỏng luồng phản hồi thông báo gốc của API
                _ = ex;
            }
        }

        /// <summary>
        /// Sinh mã định danh EasyMartCode ngẫu nhiên duy nhất cho chuỗi đại lý mới.
        /// </summary>
        private static string GenerateEasyMartCode()
        {
            return Guid.NewGuid().ToString("N")[..8].ToLower();
        }

        /// <summary>
        /// Lấy thông tin người dùng hiện tại theo UserID.
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