using BASE.Service.Core.BL;
using EasyMart.BLBase;
using EasyMart.DL.Dictionary;

namespace EasyMart.BL.Dictionary
{
    public class BLInventoryItem : BLBaseDictionary<DLInventoryItem>
    {
        public BLInventoryItem(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }
    }
}
