using BASE.Service.Core.BL;
using BASE.Service.Core.Model;
using BASE.Service.Core.Web;
using EasyMart.BL.Dictionary;
using EasyMart.Model.Dictionary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EasyMart.DI.API.Controllers
{
    [Route("v1/inventory_item")]
    [ApiController]
    public class InventoryItemController : BaseServicesController<InventoryItem, BLInventoryItem>
    {
        public InventoryItemController(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override BLInventoryItem CreateBL(CoreWebServiceCollection serviceCollection)
        {
            return new BLInventoryItem(serviceCollection);
        }

        [HttpPost("Test")]
        public async Task<ServiceResponse> Test()
        {
            var x = await BLObject.InsertBatch();
            return x;
        }
    }
}
