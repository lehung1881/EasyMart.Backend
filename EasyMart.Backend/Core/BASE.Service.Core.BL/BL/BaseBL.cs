using BASE.Service.Core.Model;
using BASE.Service.Core.Services;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using System.Text;

namespace BASE.Service.Core.BL
{
    /// <summary>
    /// Base class cho Business Logic layer, cung cấp các chức năng CRUD cơ bản (MySQL + Dapper)
    /// </summary>
    /// <typeparam name="TModel">Model kế thừa từ BaseModel</typeparam>
    public partial class BaseBL
    {
        #region Fields & Constructor

        /// <summary>
        /// Cache column name theo Database + Table
        /// </summary>
        private static readonly Dictionary<string, List<string>> _columnCache = new();

        /// <summary>
        /// Lock object đảm bảo thread-safe khi truy cập _columnCache.
        /// </summary>
        private static readonly object _columnLock = new();

        /// <summary>
        /// Collection các service được inject vào BL thông qua DI.
        /// </summary>
        protected readonly CoreWebServiceCollection _serviceCollection;

        /// <summary>
        /// Service xác thực, dùng để lấy thông tin người dùng từ HTTP header.
        /// </summary>
        protected IAuthService _authService => _serviceCollection.AuthService();

        /// <summary>
        /// Service thao tác với MySQL.
        /// </summary>
        protected IMySQLService _mySQLService => _serviceCollection.MySQLService();

        /// <summary>
        /// Cache service
        /// </summary>
        protected ICacheService _cacheService => _serviceCollection.CacheService();

        /// <summary>
        /// Khởi tạo BaseBL với service collection được inject từ controller.
        /// </summary>
        /// <param name="serviceCollection">Collection chứa các service cần thiết.</param>
        protected BaseBL(CoreWebServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        private Guid _userID = Guid.Empty;

        /// <summary>
        /// ID của người dùng hiện tại, lấy từ header "X-UserID".
        /// Được khởi tạo lazy: chỉ gọi AuthService một lần, các lần sau lấy từ cache.
        /// </summary>
        protected Guid UserID
        {
            get
            {
                if (_userID == Guid.Empty)
                {
                    _userID = _authService.GetUserID();
                }
                return _userID;
            }
        }

        private Guid _tenantID = Guid.Empty;

        /// <summary>
        /// ID của tenant hiện tại, lấy từ header "X-TenantID".
        /// Được khởi tạo lazy: chỉ gọi AuthService một lần, các lần sau lấy từ cache.
        /// </summary>
        protected Guid TenantID
        {
            get
            {
                if (_tenantID == Guid.Empty)
                {
                    _tenantID = _authService.GetTenantID();
                }
                return _tenantID;
            }
        }

        private Guid _databaseID = Guid.Empty;

        /// <summary>
        /// ID database của tenant hiện tại, lấy từ header "X-DatabaseID".
        /// Được khởi tạo lazy: chỉ gọi AuthService một lần, các lần sau lấy từ cache.
        /// </summary>
        protected Guid DatabaseID
        {
            get
            {
                if (_databaseID == Guid.Empty)
                {
                    _databaseID = _authService.GetDatabaseID();
                }
                return _databaseID;
            }
        }

        private string _fullName = string.Empty;

        /// <summary>
        /// Họ tên của người dùng hiện tại, lấy từ header "X-FullName".
        /// Được khởi tạo lazy: chỉ gọi AuthService một lần, các lần sau lấy từ cache.
        /// </summary>
        protected string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(_fullName))
                {
                    _fullName = _authService.GetFullName();
                }
                return _fullName;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Lấy connection theo databaseID (customer DB)
        /// </summary>
        protected virtual async Task<IDbConnection> GetConnectionAsync()
        {
            return await _mySQLService.GetDBConnectionAsync(DatabaseID);
        }

        /// <summary>
        /// Lấy bản ghi theo ID.
        /// </summary>
        /// <typeparam name="T">Kiểu model kế thừa từ BaseModel.</typeparam>
        /// <param name="id">ID của bản ghi cần lấy.</param>
        /// <returns>Bản ghi tương ứng với ID hoặc null nếu không tìm thấy.</returns>
        public async Task<T> GetDataByID<T>(string id) where T : BaseModel
        {
            return await _mySQLService.GetDataByID<T>(DatabaseID, id);
        }

        /// <summary>
        /// Lấy bản ghi theo ID với kiểu model động.
        /// </summary>
        /// <param name="modelType">Kiểu model cần lấy.</param>
        /// <param name="id">ID của bản ghi cần lấy.</param>
        /// <param name="columns">Danh sách cột cần lấy, mặc định là tất cả (*).</param>
        /// <returns>Bản ghi tương ứng với ID hoặc null nếu không tìm thấy.</returns>
        public async Task<object> GetDataByID(Type modelType, string id, string columns = "*")
        {
            return await _mySQLService.GetDataByID(DatabaseID, modelType, id);
        }

        /// <summary>
        /// Lấy danh sách bản ghi có phân trang.
        /// </summary>
        /// <returns><see cref="PagingResponse"/> chứa danh sách bản ghi và thông tin phân trang.</returns>
        public async Task<PagingResponse> GetDataPaging(PagingRequest pagingRequest)
        {
            return await _mySQLService.GetDataPaging(DatabaseID, pagingRequest);
        }

        /// <summary>
        /// Lấy dữ liệu Master kèm theo các Detail được cấu hình trong ModelDetailConfigs.
        /// Tận dụng QueryMultiple để gom tất cả queries vào 1 round-trip duy nhất.
        /// </summary>
        /// <param name="modelType">Kiểu model của Master.</param>
        /// <param name="id">ID của bản ghi Master.</param>
        /// <param name="columns">Danh sách cột cần lấy cho Master, mặc định là tất cả (*).</param>
        /// <returns>
        /// Object Master đã được gán dữ liệu Detail vào các property tương ứng,
        /// hoặc null nếu không tìm thấy Master.
        /// </returns>
        public async Task<object> GetMasterDetail(Type modelType, string id, string columns = "*")
        {
            var tempInstance = (BaseModel)Activator.CreateInstance(modelType);
            var detailConfigs = tempInstance.ModelDetailConfigs;

            if (detailConfigs == null || detailConfigs.Count == 0)
            {
                return await GetDataByID(modelType, id, columns);
            }

            var sqlBuilder = new StringBuilder();
            var param = new Dictionary<string, object> { { "MasterID", id } };

            string masterPkField = modelType.GetPrimaryKeyFieldName();
            string masterTable = modelType.GetViewOrTableName();
            sqlBuilder.AppendLine($"SELECT {columns} FROM `{masterTable}` WHERE `{masterPkField}` = @MasterID LIMIT 1;");

            // Resolve đúng item type từ property List<T> trên master
            var masterProps = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var detailItemTypes = new List<Type>();

            foreach (var cfg in detailConfigs)
            {
                sqlBuilder.AppendLine(
                    $"SELECT * FROM `{cfg.DetailTableName}` WHERE `{cfg.ForeignKeyName}` = @MasterID;"
                );

                // Lấy type T từ property List<T>
                var prop = masterProps.FirstOrDefault(p =>
                    string.Equals(p.Name, cfg.PropertyOnMasterModel, StringComparison.OrdinalIgnoreCase));

                Type itemType = typeof(object);
                if (prop != null && prop.PropertyType.IsGenericType)
                {
                    // Lấy đúng Type của detail
                    itemType = prop.PropertyType.GetGenericArguments()[0];
                }

                detailItemTypes.Add(itemType);
            }

            // types = [masterModel, detailType1, detailType2, ...]
            var types = new List<Type> { modelType };
            types.AddRange(detailItemTypes);

            var results = await _mySQLService.QueryMultipleUsingCommandText(
                DatabaseID,
                sqlBuilder.ToString(),
                types,
                param
            );

            var masterList = results[0];
            if (masterList == null || masterList.Count == 0)
                return null;

            var master = (BaseModel)masterList[0];

            // Cache reflection methods — chỉ resolve 1 lần
            var castMethod = typeof(Enumerable).GetMethod("Cast")!;
            var toListMethod = typeof(Enumerable).GetMethod("ToList")!;

            for (int i = 0; i < detailConfigs.Count; i++)
            {
                var cfg = detailConfigs[i];
                var itemType = detailItemTypes[i];

                var prop = masterProps.FirstOrDefault(p =>
                    string.Equals(p.Name, cfg.PropertyOnMasterModel, StringComparison.OrdinalIgnoreCase));

                if (prop == null) continue;

                // MakeGenericMethod chỉ tốn cost khi itemType khác nhau
                var castedList = toListMethod.MakeGenericMethod(itemType).Invoke(null, new object[] {
                    castMethod.MakeGenericMethod(itemType).Invoke(null, new object[] { results[i + 1] })
                });

                prop.SetValue(master, castedList);
            }

            return master;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Lấy danh sách properties có thể map với cột trong DB.
        /// Loại bỏ các property có attribute [NotMapped] hoặc không tồn tại trong DB.
        /// </summary>
        /// <param name="modelType">Kiểu model cần lấy properties.</param>
        /// <param name="dbColumnSet">Tập hợp tên cột thực tế trong DB.</param>
        /// <returns>Danh sách properties có thể map.</returns>
        private static List<PropertyInfo> GetMappableProperties(Type modelType, HashSet<string> dbColumnSet)
        {
            return modelType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null)
                .Where(p => dbColumnSet.Contains(p.Name))
                .ToList();
        }

        /// <summary>
        /// Đảm bảo Primary Key được sinh tự động nếu là kiểu Guid và chưa có giá trị.
        /// </summary>
        /// <param name="model">Model cần kiểm tra và gán PK.</param>
        /// <param name="pkProp">PropertyInfo của trường Primary Key.</param>
        private static void EnsurePrimaryKey(BaseModel model, PropertyInfo pkProp)
        {
            if (model.IsNullOrEmptyPrimary())
            {
                model.SetAutoPrimaryKey();
            }
        }

        /// <summary>
        /// Lấy danh sách properties cần update, loại bỏ Primary Key.
        /// Nếu có chỉ định <paramref name="updateColumns"/>, chỉ lấy những cột đó.
        /// </summary>
        /// <param name="allProps">Toàn bộ properties có thể map của model.</param>
        /// <param name="updateColumns">Danh sách tên cột cần update, null hoặc rỗng để update tất cả.</param>
        /// <param name="primaryKeyName">Tên trường Primary Key cần loại bỏ.</param>
        /// <returns>Danh sách properties sẽ được dùng trong câu lệnh UPDATE.</returns>
        private static IEnumerable<PropertyInfo> GetUpdateProperties(
            List<PropertyInfo> allProps,
            List<string> updateColumns,
            string primaryKeyName)
        {
            var propsExcludePk = allProps
                .Where(p => !string.Equals(p.Name, primaryKeyName, StringComparison.OrdinalIgnoreCase));

            if (updateColumns != null && updateColumns.Any())
            {
                var updateSet = new HashSet<string>(updateColumns, StringComparer.OrdinalIgnoreCase);
                return propsExcludePk.Where(p => updateSet.Contains(p.Name));
            }

            return propsExcludePk;
        }

        /// <summary>
        /// Build dictionary parameters cho Dapper từ danh sách properties và giá trị của model.
        /// </summary>
        /// <param name="props">Danh sách properties cần lấy giá trị.</param>
        /// <param name="model">Model chứa dữ liệu.</param>
        /// <returns>Dictionary key-value dùng làm tham số cho câu lệnh SQL.</returns>
        private static Dictionary<string, object> BuildParameters(List<PropertyInfo> props, BaseModel model)
        {
            var parameters = new Dictionary<string, object>(props.Count, StringComparer.OrdinalIgnoreCase);

            foreach (var prop in props)
            {
                parameters[prop.Name] = prop.GetValue(model);
            }

            return parameters;
        }

        #endregion
    }
}