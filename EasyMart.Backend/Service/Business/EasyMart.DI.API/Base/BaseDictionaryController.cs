//using BASE.Service.Core.BL;
//using BASE.Service.Core.Model;
//using BASE.Service.Core.Web;
//using EasyMart.BL.Dictionary;
//using EasyMart.BLBase;
//using EasyMart.Model.Dictionary;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;

//namespace EasyMart.Business.API.Base
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class BaseDictionaryController<TModel, TBL> : BaseServicesController<TModel, TBL> where TModel : BaseModel where TBL : BaseBL
//    {
//        public BaseDictionaryController(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
//        {
//        }

//        public override TBL CreateBL(CoreWebServiceCollection serviceCollection)
//        {
//            return new BLBaseDictionary<>(serviceCollection);
//        }
//    }
//}
