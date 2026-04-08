using BASE.Service.Core.BL;
using EasyMart.BLBase;
using EasyMart.DL.Dictionary;

namespace EasyMart.BL.Dictionary
{
    public class BLSupplier : BLBaseDictionary<DLSupplier>
    {
        public BLSupplier(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override DLSupplier CreateDL()
        {
            return new DLSupplier(_mySQLService);
        }
    }
}
