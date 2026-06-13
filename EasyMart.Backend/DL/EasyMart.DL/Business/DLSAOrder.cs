using BASE.Service.Core.Services;
using EasyMart.DLBase;

namespace EasyMart.DL.Business
{
    public class DLSAOrder : DLBaseEasyMart
    {
        public DLSAOrder(IMySQLService databaseService) : base(databaseService)
        {
        }
    }
}
