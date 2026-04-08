using BASE.Service.Core.Services;
using EasyMart.DLBase;

namespace EasyMart.DL.Dictionary
{
    public class DLStock : DLBaseEasyMart
    {
        public DLStock(IMySQLService databaseService) : base(databaseService)
        {
        }
    }
}
