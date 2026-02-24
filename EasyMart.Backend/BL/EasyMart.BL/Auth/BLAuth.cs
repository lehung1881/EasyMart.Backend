using BASE.Service.Core.BL;
using BASE.Service.Core.Model;
using BASE.Service.Core.Utils;
using EasyMart.BLBase;
using EasyMart.DL.Auth;
using EasyMart.Model;

namespace EasyMart.BL.Auth
{
    public class BLAuth : BLBaseEasyMart<DLAuth>
    {
        private readonly JwtHelper _jwtHelper;

        private DLAuth _userRepo;

        public BLAuth(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
            _userRepo = new DLAuth(_mySQLService);
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // ✅ Rule 1: Kiểm tra tài khoản tồn tại
            var user = await _userRepo.GetByEmailAsync(request.Email);
            if (user is null)
            {
                // Dùng message chung để tránh lộ thông tin (User Enumeration Attack)
                throw new Exception("Tài khoản hoặc mật khẩu không chính xác");
            }

            // ✅ Rule 2: Kiểm tra tài khoản có bị khóa không
            if (user.IsLocked)
            {
                throw new Exception("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên");
            }

            // ✅ Rule 3: Kiểm tra tài khoản đã được kích hoạt chưa
            if (!user.IsActive)
            {
                throw new Exception("Tài khoản chưa được kích hoạt. Vui lòng kiểm tra email");
            }

            // ✅ Rule 4: Xác minh mật khẩu bằng BCrypt
            bool isPasswordValid = CommonFn.VerifyPassword(request.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                // Tăng số lần đăng nhập sai
                await HandleFailedLoginAsync(user);
                throw new Exception("Tài khoản hoặc mật khẩu không chính xác");
            }

            // ✅ Rule 5: Reset số lần đăng nhập sai (nếu đăng nhập thành công)
            await ResetFailedLoginAsync(user);

            // ✅ Tạo JWT Token
            var userInfo = new UserInfo
            {
                UserID = user.UserID,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
            };

            var accessToken = _jwtHelper.GenerateAccessToken(userInfo);
            var refreshToken = _jwtHelper.GenerateRefreshToken();

            // ✅ Lưu Refresh Token vào DB
            await _userRepo.SaveRefreshTokenAsync(user.UserID, refreshToken, DateTime.UtcNow.AddDays(7));

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                User = userInfo,
            };
        }

        // ============================
        // Private Helpers
        // ============================

        /// <summary>
        /// Xử lý khi đăng nhập sai: tăng counter, khóa nếu vượt ngưỡng
        /// </summary>
        private async Task HandleFailedLoginAsync(User user)
        {
            user.FailedLoginCount++;

            // Khóa tài khoản sau 5 lần sai liên tiếp
            if (user.FailedLoginCount >= 5)
            {
                user.IsLocked = true;
            }

            await _userRepo.UpdateAsync(user);
        }

        /// <summary>
        /// Reset counter khi đăng nhập thành công
        /// </summary>
        private async Task ResetFailedLoginAsync(User user)
        {
            if (user.FailedLoginCount > 0)
            {
                user.FailedLoginCount = 0;
                await _userRepo.UpdateAsync(user);
            }
        }
    }
}
