using BASE.Service.Core.BL;
using EasyMart.DLBase;

namespace EasyMart.BLBase
{
    public class BLBaseEasyMart<TDL> : BaseBL where TDL : DLBaseEasyMart
    {
        protected BLBaseEasyMart(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }
    }
}
