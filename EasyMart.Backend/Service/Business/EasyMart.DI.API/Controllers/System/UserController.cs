using BASE.Service.Core.BL;
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
    }
}
