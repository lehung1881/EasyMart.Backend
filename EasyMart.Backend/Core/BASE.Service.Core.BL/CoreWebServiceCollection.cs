using BASE.Service.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BASE.Service.Core.BL
{
    /// <summary>
    /// Lớp cung cấp các service dùng chung cho tầng Business Layer.
    /// Hỗ trợ lazy loading và tái sử dụng instance service trong phạm vi hiện tại.
    /// </summary>
    public class CoreWebServiceCollection
    {
        /// <summary>
        /// Service provider dùng để resolve các dependency từ DI Container.
        /// </summary>
        protected IServiceProvider _serviceProvider;

        /// <summary>
        /// Khởi tạo đối tượng quản lý service.
        /// </summary>
        /// <param name="serviceProvider">Đối tượng cung cấp dependency.</param>
        public CoreWebServiceCollection(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Resolve service từ DI Container.
        /// </summary>
        /// <typeparam name="T">Kiểu service cần lấy.</typeparam>
        /// <returns>Instance service tương ứng.</returns>
        public T GetService<T>()
        {
            return _serviceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Service xác thực và phân quyền.
        /// </summary>
        private IAuthService _authService;

        /// <summary>
        /// Lấy service xác thực và phân quyền.
        /// </summary>
        /// <returns>Đối tượng <see cref="IAuthService"/>.</returns>
        public IAuthService AuthService()
        {
            if (_authService == null)
            {
                _authService = GetService<IAuthService>();
            }

            return _authService;
        }

        /// <summary>
        /// Service thao tác với cơ sở dữ liệu MySQL.
        /// </summary>
        private IMySQLService _mySQLService;

        /// <summary>
        /// Lấy service thao tác với MySQL.
        /// </summary>
        /// <returns>Đối tượng <see cref="IMySQLService"/>.</returns>
        public IMySQLService MySQLService()
        {
            if (_mySQLService == null)
            {
                _mySQLService = GetService<IMySQLService>();
            }

            return _mySQLService;
        }

        /// <summary>
        /// Service thao tác với hệ thống cache.
        /// </summary>
        private ICacheService _cacheService;

        /// <summary>
        /// Lấy service thao tác với cache.
        /// </summary>
        /// <returns>Đối tượng <see cref="ICacheService"/>.</returns>
        public ICacheService CacheService()
        {
            if (_cacheService == null)
            {
                _cacheService = GetService<ICacheService>();
            }

            return _cacheService;
        }
    }
}