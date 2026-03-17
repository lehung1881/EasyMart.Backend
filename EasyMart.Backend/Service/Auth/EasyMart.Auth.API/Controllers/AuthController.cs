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
        /// Đăng nhập
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
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
        /// </summary>
        /// <param name="request">Request chứa Refresh Token hiện tại.</param>
        /// <returns>Access Token mới, Refresh Token mới và thông tin User.</returns>
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

    }
}
