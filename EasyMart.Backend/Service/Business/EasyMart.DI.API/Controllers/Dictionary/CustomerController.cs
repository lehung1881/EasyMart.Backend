using BASE.Service.Core.BL;
using BASE.Service.Core.Web;
using EasyMart.BL.Dictionary;
using EasyMart.Model.Dictionary;
using Microsoft.AspNetCore.Mvc;

namespace EasyMart.Business.API.Controllers
{
    [Route("v1/customer")]
    [ApiController]
    public class CustomerController : BaseServicesController<Customer, BLCustomer>
    {
        public CustomerController(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override BLCustomer CreateBL(CoreWebServiceCollection serviceCollection)
        {
            return new BLCustomer(serviceCollection);
        }
    }
}
