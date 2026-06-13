using BASE.Service.Core.BL;
using BASE.Service.Core.Web;
using EasyMart.BL.Dictionary;
using EasyMart.Model.Dictionary;
using Microsoft.AspNetCore.Mvc;

namespace EasyMart.Business.API.Controllers
{
    [Route("v1/supplier")]
    [ApiController]
    public class SupplierController : BaseServicesController<Supplier, BLSupplier>
    {
        public SupplierController(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override BLSupplier CreateBL(CoreWebServiceCollection serviceCollection)
        {
            return new BLSupplier(serviceCollection);
        }
    }
}
