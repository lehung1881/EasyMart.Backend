using BASE.Service.Core.BL;
using EasyMart.BLBase;
using EasyMart.DL.System;

namespace EasyMart.BL.System
{
    public class BLRole : BLBaseEasyMart<DLRole>
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
