using BASE.Service.Core.Services;
using EasyMart.DLBase;

namespace EasyMart.DL.Dictionary
{
    public class DLInventoryItem : DLBaseEasyMart
    {
        public DLInventoryItem(IMySQLService databaseService) : base(databaseService)
        {
        }
    }
}
