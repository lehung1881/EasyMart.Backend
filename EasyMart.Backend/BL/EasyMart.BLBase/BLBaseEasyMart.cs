using BASE.Service.Core.BL;
using EasyMart.DLBase;

namespace EasyMart.BLBase
{
    /// <summary>
    /// Base Business Logic class cho EasyMart.
    /// Quản lý vòng đời của DL object theo pattern Lazy Initialization.
    /// </summary>
    /// <typeparam name="TDL">Kiểu DL kế thừa từ <see cref="DLBaseEasyMart"/></typeparam>
    public abstract class BLBaseEasyMart<TDL> : BaseBL where TDL : DLBaseEasyMart
    {
        private TDL? _dlObject;

        protected BLBaseEasyMart(CoreWebServiceCollection serviceCollection) : base(serviceCollection)
        {
        }

        /// <summary>
        /// Factory method để khởi tạo DL object.
        /// Các lớp con bắt buộc phải implement phương thức này.
        /// </summary>
        /// <returns>Instance của <typeparamref name="TDL"/></returns>
        public abstract TDL CreateDL();

        /// <summary>
        /// DL object được khởi tạo theo Lazy Initialization pattern.
        /// Chỉ tạo mới khi lần đầu tiên được truy cập.
        /// </summary>
        public TDL DLObject => _dlObject ??= CreateDL();
    }
}
