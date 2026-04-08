using BASE.Service.Core.Services;
using EasyMart.DLBase;

namespace EasyMart.DL.Dictionary
{
    public class DLUnit : DLBaseEasyMart
    {
        public DLUnit(IMySQLService databaseService) : base(databaseService)
        {
        }
    }
}
