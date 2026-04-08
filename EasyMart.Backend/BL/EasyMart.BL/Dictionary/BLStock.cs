using BASE.Service.Core.BL;
using EasyMart.BLBase;
using EasyMart.DL.Dictionary;

namespace EasyMart.BL.Dictionary
{
    public class BLStock : BLBaseDictionary<DLStock>
    {
        public BLStock(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override DLStock CreateDL()
        {
            return new DLStock(_mySQLService);
        }
    }
}
