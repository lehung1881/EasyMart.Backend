using BASE.Service.Core.BL;
using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using BASE.Service.Core.Web;
using EasyMart.BL.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EasyMart.Auth.API.Controllers
{
    [Route("v1/auth")]
    [ApiController]
    public class AuthController : BaseServicesController<User, BLAuth>
    {
        public AuthController(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override BLAuth CreateBL(CoreWebServiceCollection serviceCollection) => new BLAuth(serviceCollection);

        /// <summary>
        /// Đăng nhập tài khoản.
        /// Xác minh thông tin đăng nhập và trả về Access Token, Refresh Token, UserInfo trong body response.
        /// Client có trách nhiệm lưu token vào localStorage và gửi kèm Authorization header cho các request sau.
        /// </summary>
        /// <param name="request">Thông tin đăng nhập gồm Email và Password.</param>
        /// <returns>
        /// <see cref="LoginResponse"/> chứa AccessToken, RefreshToken và UserInfo nếu thành công.
        /// </returns>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ServiceResponse> LoginAsync([FromBody] LoginRequest request)
        {
            var res = new ServiceResponse();
            try
            {
                res = await BLObject.LoginAsync(request);
            }
            catch (Exception ex)
            {
                res.OnError(ServiceResponseCode.Exception, ex.Message, ex.Message);
            }
            return res;
        }

        /// <summary>
        /// Làm mới Access Token bằng Refresh Token hợp lệ.
        /// Áp dụng Token Rotation: Refresh Token cũ bị thu hồi và một token mới được cấp phát.
        /// Client truyền Refresh Token hiện tại trong body, nhận lại cặp token mới.
        /// </summary>
        /// <param name="request">Request chứa Refresh Token hiện tại.</param>
        /// <returns>
        /// <see cref="LoginResponse"/> chứa AccessToken mới, RefreshToken mới và UserInfo.
        /// </returns>
        [HttpPost("refresh_token")]
        [AllowAnonymous]
        public async Task<ServiceResponse> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
        {
            var res = new ServiceResponse();
            try
            {
                res = await BLObject.RefreshTokenAsync(request);
            }
            catch (Exception ex)
            {
                res.OnError(ServiceResponseCode.Exception, ex.Message, ex.Message);
            }
            return res;
        }

        /// <summary>
        /// Đăng ký tài khoản người dùng mới.
        /// </summary>
        /// <param name="request">Thông tin đăng ký.</param>
        /// <returns>Thông tin User vừa được tạo.</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ServiceResponse> RegisterAsync([FromBody] RegisterRequest request)
        {
            var res = new ServiceResponse();
            try
            {
                return await BLObject.RegisterAsync(request);
            }
            catch (Exception ex)
            {
                res.OnError(ServiceResponseCode.Exception, ex.Message, ex.Message);
            }
            return res;
        }

        /// <summary>
        /// Đăng xuất tài khoản hiện tại.
        /// Yêu cầu người dùng đã xác thực (có Access Token hợp lệ trong Authorization header).
        /// Client có trách nhiệm xóa token khỏi localStorage sau khi nhận response thành công.
        /// </summary>
        /// <returns>HTTP 200 OK nếu đăng xuất thành công.</returns>
        [HttpPost("logout")]
        //[Authorize]
        public IActionResult Logout()
        {
            return Ok();
        }

        /// <summary>
        /// Kiểm tra phiên đăng nhập hiện tại còn hợp lệ không.
        /// Xác thực Access Token từ Authorization header và trả về UserInfo nếu hợp lệ.
        /// Được gọi mỗi khi app khởi động (F5, mở tab mới) để khôi phục trạng thái đăng nhập.
        /// </summary>
        /// <returns>UserInfo nếu token hợp lệ, 401 nếu token hết hạn.</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<ServiceResponse> MeAsync()
        {
            var res = new ServiceResponse();
            try
            {
                res = await BLObject.GetUserInfoAsync(UserID);
            }
            catch (Exception ex)
            {
                res.OnError(ServiceResponseCode.Exception, ex.Message, ex.Message);
            }
            return res;
        }
    }
}