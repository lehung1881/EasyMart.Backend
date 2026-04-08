using BASE.Service.Core.Services;
using EasyMart.DLBase;

namespace EasyMart.DL.Dictionary
{
    public class DLCustomer : DLBaseEasyMart
    {
        public DLCustomer(IMySQLService databaseService) : base(databaseService)
        {
        }
    }
}
