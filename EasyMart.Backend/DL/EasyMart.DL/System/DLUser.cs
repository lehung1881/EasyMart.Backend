using BASE.Service.Core.Services;
using EasyMart.DLBase;

namespace EasyMart.DL.System
{
    public class DLUser : DLBaseEasyMart
    {
        public DLUser(IMySQLService databaseService) : base(databaseService)
        {
        }
    }
}
