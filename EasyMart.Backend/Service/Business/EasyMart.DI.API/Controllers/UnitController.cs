using BASE.Service.Core.BL;
using BASE.Service.Core.Web;
using EasyMart.BL.Dictionary;
using EasyMart.Model.Dictionary;
using Microsoft.AspNetCore.Mvc;

namespace EasyMart.DI.API.Controllers
{
    [Route("v1/unit")]
    [ApiController]
    public class UnitController : BaseServicesController<Unit, BLUnit>
    {
        public UnitController(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override BLUnit CreateBL(CoreWebServiceCollection serviceCollection)
        {
            return new BLUnit(serviceCollection);
        }
    }
}
