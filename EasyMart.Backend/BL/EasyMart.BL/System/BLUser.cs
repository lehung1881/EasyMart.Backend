using BASE.Service.Core.BL;
using EasyMart.BLBase;
using EasyMart.DL.System;

namespace EasyMart.BL.System
{
    public class BLUser : BLBaseEasyMart<DLUser>
    {
        public BLUser(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override DLUser CreateDL()
        {
            return new DLUser(_mySQLService);
        }
    }
}
