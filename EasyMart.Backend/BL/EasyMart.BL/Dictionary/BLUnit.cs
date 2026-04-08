using BASE.Service.Core.BL;
using EasyMart.BLBase;
using EasyMart.DL.Dictionary;

namespace EasyMart.BL.Dictionary
{
    public class BLUnit : BLBaseDictionary<DLUnit>
    {
        public BLUnit(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override DLUnit CreateDL()
        {
            return new DLUnit(_mySQLService);
        }
    }
}
