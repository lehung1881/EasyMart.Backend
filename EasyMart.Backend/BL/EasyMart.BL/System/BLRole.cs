using BASE.Service.Core.BL;
using EasyMart.BLBase;
using EasyMart.DL.Business;
using EasyMart.DL.Dictionary;

namespace EasyMart.BL.Dictionary
{
    public class BLRole : BLBaseDictionary<DLRole>
    {
        public BLRole(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override DLRole CreateDL()
        {
            return new DLRole(_mySQLService);
        }
    }
}
