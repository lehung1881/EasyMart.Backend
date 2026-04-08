using BASE.Service.Core.BL;
using EasyMart.BLBase;
using EasyMart.DL.Dictionary;

namespace EasyMart.BL.Dictionary
{
    public class BLCustomer : BLBaseDictionary<DLCustomer>
    {
        public BLCustomer(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override DLCustomer CreateDL()
        {
            return new DLCustomer(_mySQLService);
        }
    }
}
