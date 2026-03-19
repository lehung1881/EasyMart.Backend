using BASE.Service.Core.BL;
using EasyMart.DLBase;

namespace EasyMart.BLBase
{
    public abstract class BLBaseDictionary<TDL> : BLBaseEasyMart<TDL> where TDL : DLBaseEasyMart
    {
        public BLBaseDictionary(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }
    }
}
