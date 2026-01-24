using BASE.Service.Core.Services;

namespace EasyMart.DLBase
{
    public class DLBaseEasyMart
    {
        /// <summary>
        /// Đối tượng thao tác với Database
        /// </summary>
        protected readonly IMySQLService _databaseService;
        public DLBaseEasyMart(IMySQLService databaseService)
        {
            _databaseService = databaseService;
        }

    }
}
