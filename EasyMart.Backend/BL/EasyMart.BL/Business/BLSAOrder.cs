using BASE.Service.Core.BL;
using EasyMart.BLBase;
using EasyMart.DL.Business;

namespace EasyMart.BL.Business
{
    /// <summary>
    /// BL Xử lý nghiệp vụ Đơn hàng
    /// </summary>
    public class BLSAOrder : BLBaseDictionary<DLSAOrder>
    {
        public BLSAOrder(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        public override DLSAOrder CreateDL()
        {
            return new DLSAOrder(_mySQLService);
        }
    }
}
