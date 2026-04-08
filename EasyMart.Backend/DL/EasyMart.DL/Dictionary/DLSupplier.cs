using BASE.Service.Core.Services;
using EasyMart.DLBase;

namespace EasyMart.DL.Dictionary
{
    public class DLSupplier : DLBaseEasyMart
    {
        public DLSupplier(IMySQLService databaseService) : base(databaseService)
        {
        }
    }
}
