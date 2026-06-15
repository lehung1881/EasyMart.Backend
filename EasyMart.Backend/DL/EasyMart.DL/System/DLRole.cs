using BASE.Service.Core.Services;
using EasyMart.DLBase;

namespace EasyMart.DL.System
{
    public class DLRole : DLBaseEasyMart
    {
        public DLRole(IMySQLService databaseService) : base(databaseService)
        {
        }
    }
}
