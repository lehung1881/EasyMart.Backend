using BASE.Service.Core.BL;
using BASE.Service.Core.Web;
using EasyMart.BL.Business;
using EasyMart.Model.Dictionary;
using Microsoft.AspNetCore.Mvc;

namespace EasyMart.Business.API.Controllers
{
    [Route("v1/sa_order")]
    [ApiController]
    public class SAOrderController : BaseServicesController<SAOrder, BLSAOrder>
    {
        public SAOrderController(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override BLSAOrder CreateBL(CoreWebServiceCollection serviceCollection)
        {
            return new BLSAOrder(serviceCollection);
        }
    }
}
