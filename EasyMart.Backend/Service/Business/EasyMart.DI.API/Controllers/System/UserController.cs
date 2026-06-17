using BASE.Service.Core.BL;
using BASE.Service.Core.Model;
using BASE.Service.Core.Web;
using EasyMart.BL.System;
using EasyMart.Model.System;
using Microsoft.AspNetCore.Mvc;

namespace EasyMart.Business.API.Controllers
{
    [Route("v1/user")]
    [ApiController]
    public class UserController : BaseServicesController<SysMscUser, BLUser>
    {
        public UserController(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override BLUser CreateBL(CoreWebServiceCollection serviceCollection)
        {
            return new BLUser(serviceCollection);
        }

        /// <summary>
        /// Lấy danh sách quyền của từng User theo ngữ cảnh đăng nhập
        /// </summary>
        /// <returns>ServiceResponse chứa danh sách SysMscPermissionMapping</returns>
        [HttpGet("permissions")]
        public async Task<ServiceResponse> GetUserPermissions()
        {
            var response = new ServiceResponse();
            try
            {
                // 1. Gọi xuống hàm xử lý ở tầng Business Logic (Hàm này đã được bọc Cache tự động)
                var permissions = await BLObject.GetPermissionUserAsync();

                // 2. Gán dữ liệu thành công vào ServiceResponse qua hàm OnSuccess
                response.OnSuccess(permissions);
            }
            catch (Exception ex)
            {
                response.OnError($"Có lỗi xảy ra trong quá trình lấy quyền người dùng, Ex: {ex.Message}.");
            }

            return response;
        }
    }
}
