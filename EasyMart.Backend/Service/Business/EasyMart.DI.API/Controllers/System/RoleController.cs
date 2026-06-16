using BASE.Service.Core.BL;
using BASE.Service.Core.Web;
using EasyMart.BL.Library;
using EasyMart.BL.System;
using EasyMart.Model.System;
using Microsoft.AspNetCore.Mvc;

namespace EasyMart.Business.API.Controllers
{
    [Route("v1/role")]
    [ApiController]
    [PermissionFilter("aa")]
    public class RoleController : BaseServicesController<SysMscRole, BLRole>
    {
        public RoleController(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override BLRole CreateBL(CoreWebServiceCollection serviceCollection)
        {
            return new BLRole(serviceCollection);
        }
    }
}
