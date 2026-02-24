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
                var response = await BLObject.LoginAsync(request);
                res.OnSuccess(response);
            }
            catch (Exception ex)
            {
                res.OnError(ServiceResponseCode.Exception, ex.Message, ex.Message);
            }
            return res;
        }

    }
}
