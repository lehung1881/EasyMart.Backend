using BASE.Service.Core.BL;
using BASE.Service.Core.Web;
using EasyMart.BL.Dictionary;
using EasyMart.Model.Dictionary;
using Microsoft.AspNetCore.Mvc;

namespace EasyMart.Business.API.Controllers
{
    [Route("v1/stock")]
    [ApiController]
    public class StockController : BaseServicesController<Stock, BLStock>
    {
        public StockController(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override BLStock CreateBL(CoreWebServiceCollection serviceCollection)
        {
            return new BLStock(serviceCollection);
        }
    }
}
