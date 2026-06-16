using BASE.Service.Core.BL;
using EasyMart.BLBase;
using EasyMart.DL.System;
using EasyMart.Model.System;

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

        public async Task GetUsercache()
        {
            await _cacheService.SetAsync("hihi", new SysMscUser());

            var s = await _cacheService.GetAsync<SysMscUser>("hihi");
        }
    }
}
