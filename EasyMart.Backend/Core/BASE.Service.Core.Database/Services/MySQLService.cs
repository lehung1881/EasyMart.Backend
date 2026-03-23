using System.Collections.Concurrent;
using System.Data;
using System.Text;
using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using BASE.Service.Core.Services;
using Dapper;
using MySqlConnector;

namespace BASE.Service.Core.Database
{
    public class MySQLService : IMySQLService
    {
        // Cache để lưu trữ TenantDatabase theo DatabaseID
        private static readonly ConcurrentDictionary<Guid, TenantDatabase> _databaseConfigCache = new ConcurrentDictionary<Guid, TenantDatabase>();

        public MySQLService()
        {
        }

        #region Methods connect

        /// <summary>
        /// Lấy chuỗi kết nối của customer từ master DB với cache
        /// </summary>
        /// <param name="databaseID">ID của customer database</param>
        /// <returns>Connection string tương ứng</returns>
        public async Task<TenantDatabase> GetDatabaseConfig(Guid databaseID)
        {
            // Kiểm tra cache trước
            if (_databaseConfigCache.TryGetValue(databaseID, out var cachedConfig))
            {
                return cachedConfig;
            }

            var masterConnectionString = GetMasterConnectionString();
            IDbConnection masterConnection = new MySqlConnection(masterConnectionString);

            try
            {
                masterConnection.Open();
                const string sql = @"SELECT * FROM tenant_database WHERE DatabaseID = @DatabaseID AND Status = 0 LIMIT 1;";
                var databaseConfig = await masterConnection.QueryFirstOrDefaultAsync<TenantDatabase>(sql, new { DatabaseID = databaseID });

                if (databaseConfig == null)
                {
                    throw new InvalidOperationException($"Connection string not found for databaseID: {databaseID}");
                }

                // Lưu vào cache
                _databaseConfigCache.TryAdd(databaseID, databaseConfig);

                return databaseConfig;
            }
            finally
            {
                if (masterConnection.State == ConnectionState.Open)
                {
                    masterConnection.Close();
                }
                masterConnection.Dispose();
            }
        }

        /// <summary>
        /// Xóa cache của một database config cụ thể
        /// </summary>
        /// <param name="databaseID">ID của database cần xóa cache</param>
        public void ClearDatabaseConfigCache(Guid databaseID)
        {
            _databaseConfigCache.TryRemove(databaseID, out _);
        }

        /// <summary>
        /// Xóa toàn bộ cache database config
        /// </summary>
        public void ClearAllDatabaseConfigCache()
        {
            _databaseConfigCache.Clear();
        }


        /// <summary>
        /// Lấy cấu hình DB Master
        /// </summary>
        /// <returns></returns>
        private string GetMasterConnectionString()
        {
            return GlobalConfig.AppSettings.ConnectionStrings.MasterDB;
        }

        /// <summary>
        /// Lấy kết nối MySQL
        /// </summary>
        /// <param name="databaseID"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<IDbConnection> GetDBConnectionAsync(Guid databaseID)
        {
            // Nếu là MasterDB
            if (databaseID == Constants.MasterDatabaseID)
            {
                var masterConnectionString = GetMasterConnectionString();
                return new MySqlConnection(masterConnectionString);
            }

            var dbConfig = await GetDatabaseConfig(databaseID);

            if (dbConfig == null)
            {
                throw new Exception("Database is null or empty.");
            }

            var cnnDBStringBuilder = new MySqlConnectionStringBuilder()
            {
                Port = dbConfig.Port != null ? (uint)dbConfig.Port : 3306,
                Server = dbConfig.Server,
                Database = dbConfig.Database,
                UserID = dbConfig.UserID,
                Password = dbConfig.Password,
                SslMode = MySqlSslMode.Disabled,
                AllowUserVariables = true,
                MaximumPoolSize = 200
            };

            var cnn = new MySqlConnection(cnnDBStringBuilder.ToString());
            return cnn;
        }

        /// <summary>
        /// Mở kết nối
        /// </summary>
        /// <param name="cnn"></param>
        public void OpenConnection(IDbConnection cnn)
        {
            if (cnn.State != ConnectionState.Open)
            {
                cnn.Open();
            }
        }

        /// <summary>
        /// Đóng kết nối
        /// </summary>
        /// <param name="cnn"></param>
        public void CloseConnection(IDbConnection cnn)
        {
            if (cnn != null)
            {
                if (cnn.State != ConnectionState.Closed)
                {
                    cnn.Close();
                }
                cnn.Dispose();
            }
        }

        #endregion

        #region Methods query text
        /// <summary>
        /// Thực hiện truy vấn dữ liệu từ database sử dụng command text
        /// </summary>
        public async Task<List<T>> QueryUsingCommandText<T>(Guid databaseID, string commandText, Dictionary<string, object> param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = await GetDBConnectionAsync(databaseID);
                var dynamicParams = ConvertToDynamicParameters(param);
                var result = await cnn.QueryAsync<T>(sql: commandText, param: dynamicParams, commandType: CommandType.Text);
                return result.AsList();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        /// <summary>
        /// Thực hiện truy vấn dữ liệu từ database sử dụng command text với connection có sẵn
        /// </summary>
        public async Task<List<T>> QueryUsingCommandText<T>(IDbConnection cnn, string commandText, Dictionary<string, object> param)
        {
            try
            {
                var dynamicParams = ConvertToDynamicParameters(param);
                var result = await cnn.QueryAsync<T>(sql: commandText, param: dynamicParams, commandType: CommandType.Text);
                return result.ToList();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        /// <summary>
        /// Thực hiện truy vấn nhiều tập kết quả từ database sử dụng command text
        /// </summary>
        public async Task<List<List<object>>> QueryMultipleUsingCommandText(Guid databaseID, string commandText, List<Type> types, Dictionary<string, object> param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = await GetDBConnectionAsync(databaseID);
                var dynamicParams = ConvertToDynamicParameters(param);
                var multi = await cnn.QueryMultipleAsync(sql: commandText, param: dynamicParams, commandType: CommandType.Text);
                return await ReadMultipleResults(multi, types);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        /// <summary>
        /// Thực hiện truy vấn nhiều tập kết quả từ database sử dụng command text với connection có sẵn
        /// </summary>
        public async Task<List<List<object>>> QueryMultipleUsingCommandText(IDbConnection cnn, string commandText, List<Type> types, Dictionary<string, object> param)
        {
            try
            {
                var dynamicParams = ConvertToDynamicParameters(param);
                var multi = await cnn.QueryMultipleAsync(sql: commandText, param: dynamicParams, commandType: CommandType.Text);
                return await ReadMultipleResults(multi, types);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        /// <summary>
        /// Thực thi lệnh SQL (INSERT, UPDATE, DELETE) sử dụng command text
        /// </summary>
        public async Task<bool> ExecuteUsingCommandText(Guid databaseID, string commandText, Dictionary<string, object> param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = await GetDBConnectionAsync(databaseID);
                var dynamicParams = ConvertToDynamicParameters(param);
                var rowsAffected = await cnn.ExecuteAsync(sql: commandText, param: dynamicParams, commandType: CommandType.Text);
                return rowsAffected > 0;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        /// <summary>
        /// Thực thi lệnh SQL (INSERT, UPDATE, DELETE) sử dụng command text với connection có sẵn
        /// </summary>
        public async Task<bool> ExecuteUsingCommandText(IDbConnection cnn, string commandText, Dictionary<string, object> param)
        {
            try
            {
                var dynamicParams = ConvertToDynamicParameters(param);
                var rowsAffected = await cnn.ExecuteAsync(sql: commandText, param: dynamicParams, commandType: CommandType.Text);
                return rowsAffected > 0;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        /// <summary>
        /// Thực thi lệnh SQL trong một transaction có sẵn.
        /// Không đóng connection sau khi thực thi — việc commit/rollback/dispose
        /// do caller quản lý.
        /// </summary>
        public async Task<bool> ExecuteUsingCommandText(IDbConnection cnn, IDbTransaction transaction, string commandText, Dictionary<string, object> param)
        {
            try
            {
                var dynamicParams = ConvertToDynamicParameters(param);
                var rowsAffected = await cnn.ExecuteAsync(
                    sql: commandText,
                    param: dynamicParams,
                    transaction: transaction,
                    commandType: CommandType.Text
                );
                return rowsAffected > 0;
            }
            catch
            {
                throw;
            }
        }

        #endregion

        #region Paging data
        /// <summary>
        /// Lấy dữ liệu phân trang từ PagingRequest.
        /// Build SQL, thực thi truy vấn và trả về PagingResponse.
        /// </summary>
        /// <param name="databaseID">ID của database cần truy vấn.</param>
        /// <param name="request">Yêu cầu phân trang, lọc, sắp xếp.</param>
        /// <returns>PagingResponse chứa dữ liệu trang và tổng số bản ghi.</returns>
        public async Task<PagingResponse> GetDataPaging(Guid databaseID, PagingRequest request)
        {
            // Build câu SQL và parameters
            var sqlResult = GenerateSqlPaging(request);

            // Ghép 2 câu SQL thành 1 command text để query multiple
            var commandText = $"{sqlResult.PagingQuery}; {sqlResult.PagingQueryCount}";

            // Định nghĩa kiểu trả về cho từng result set
            var types = new List<Type>
            {
                typeof(object),  // Result set 1: dữ liệu trang
                typeof(int)      // Result set 2: tổng số bản ghi
            };

            // Thực thi query multiple
            var results = await QueryMultipleUsingCommandText(databaseID, commandText, types, sqlResult.Parameters);

            // Lấy kết quả từ 2 result set
            var pageData = results[0];
            var total = results[1]?.FirstOrDefault() != null ? Convert.ToInt32(results[1].FirstOrDefault()) : 0;

            return new PagingResponse(pageData, total);
        }

        public PagingSQLBuilder GenerateSqlPaging(PagingRequest request)
        {
            var parameters = new Dictionary<string, object>();
            var whereClause = BuildWhereClause(request.Filter, parameters);
            var orderByClause = BuildOrderByClause(request.Sort);
            var columns = string.IsNullOrWhiteSpace(request.Columns) ? "*" : request.Columns;
            var tableName = request.ViewOrTableName;

            var hasSelectedValue = request.SelectedValue != null
                && !string.IsNullOrWhiteSpace(request.SelectedValue.Property)
                && request.SelectedValue.Value != null;

            // Build WHERE clause 
            string finalWhereClause;
            if (hasSelectedValue)
            {
                var selectedParamName = "@selectedValue";
                var selectedProperty = $"`{request.SelectedValue.Property}`";
                parameters[selectedParamName] = request.SelectedValue.Value.ToString();

                var selectedCondition = $"{selectedProperty} = {selectedParamName}";

                if (!string.IsNullOrEmpty(whereClause))
                {
                    finalWhereClause = $"({whereClause}) OR ({selectedCondition})";
                }
                else
                {
                    finalWhereClause = string.Empty;
                }
            }
            else
            {
                finalWhereClause = whereClause;
            }

            // Build ORDER BY
            string finalOrderBy;
            if (hasSelectedValue)
            {
                var selectedParamName = "@selectedValue";
                var selectedProperty = $"`{request.SelectedValue.Property}`";

                var caseWhenExpr = $"CASE WHEN {selectedProperty} = {selectedParamName} THEN 0 ELSE 1 END ASC";

                finalOrderBy = !string.IsNullOrEmpty(orderByClause)
                    ? $"{caseWhenExpr}, {orderByClause}"
                    : caseWhenExpr;
            }
            else
            {
                finalOrderBy = orderByClause;
            }

            // Build SQL
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"SELECT {columns} FROM `{tableName}`");

            if (!string.IsNullOrEmpty(finalWhereClause))
            {
                sqlBuilder.Append($" WHERE {finalWhereClause}");
            }

            if (!string.IsNullOrEmpty(finalOrderBy))
            {
                sqlBuilder.Append($" ORDER BY {finalOrderBy}");
            }

            var offset = (request.PageIndex - 1) * request.PageSize;
            sqlBuilder.Append($" LIMIT @PageSize OFFSET @Offset");
            parameters["@Offset"] = offset;
            parameters["@PageSize"] = request.PageSize;

            // Build Count SQL
            var countBuilder = new StringBuilder();
            countBuilder.Append($"SELECT COUNT(*) FROM `{tableName}`");

            if (!string.IsNullOrEmpty(finalWhereClause))
            {
                countBuilder.Append($" WHERE {finalWhereClause}");
            }

            return new PagingSQLBuilder
            {
                PagingQuery = sqlBuilder.ToString(),
                PagingQueryCount = countBuilder.ToString(),
                Parameters = parameters
            };
        }


        /// <summary>
        /// Build mệnh đề WHERE từ danh sách FilterCondition.
        /// </summary>
        private string BuildWhereClause(List<FilterCondition> filters, Dictionary<string, object> parameters)
        {
            if (filters == null || filters.Count == 0)
                return string.Empty;

            var conditions = new List<string>();

            for (int i = 0; i < filters.Count; i++)
            {
                var filter = filters[i];
                var paramName = $"@p{i}";
                var condition = BuildSingleCondition(filter, paramName, parameters, i);

                if (!string.IsNullOrEmpty(condition))
                {
                    conditions.Add(condition);
                }
            }

            return conditions.Count > 0 ? string.Join(" AND ", conditions) : string.Empty;
        }

        /// <summary>
        /// Build một điều kiện lọc đơn lẻ (MySQL syntax).
        /// </summary>
        private string BuildSingleCondition(FilterCondition filter, string paramName, Dictionary<string, object> parameters, int index)
        {
            var property = $"`{filter.Property}`";

            switch (filter.Operator)
            {
                case FilterOperator.Equal:
                    parameters[paramName] = filter.Value;
                    return $"{property} = {paramName}";

                case FilterOperator.NotEqual:
                    parameters[paramName] = filter.Value;
                    return $"{property} <> {paramName}";

                case FilterOperator.Contains:
                    parameters[paramName] = $"%{filter.Value}%";
                    return $"{property} LIKE {paramName}";

                case FilterOperator.NotContains:
                    parameters[paramName] = $"%{filter.Value}%";
                    return $"{property} NOT LIKE {paramName}";

                case FilterOperator.StartsWith:
                    parameters[paramName] = $"{filter.Value}%";
                    return $"{property} LIKE {paramName}";

                case FilterOperator.EndsWith:
                    parameters[paramName] = $"%{filter.Value}";
                    return $"{property} LIKE {paramName}";

                case FilterOperator.IsNullOrEmpty:
                    return $"({property} IS NULL OR {property} = '')";

                case FilterOperator.IsNotNullOrEmpty:
                    return $"({property} IS NOT NULL AND {property} <> '')";

                case FilterOperator.LessThan:
                    parameters[paramName] = filter.Value;
                    return $"{property} < {paramName}";

                case FilterOperator.LessThanOrEqual:
                    parameters[paramName] = filter.Value;
                    return $"{property} <= {paramName}";

                case FilterOperator.GreaterThan:
                    parameters[paramName] = filter.Value;
                    return $"{property} > {paramName}";

                case FilterOperator.GreaterThanOrEqual:
                    parameters[paramName] = filter.Value;
                    return $"{property} >= {paramName}";

                case FilterOperator.In:
                    if (filter.Value is IEnumerable<object> inValues)
                    {
                        var inParams = new List<string>();
                        int j = 0;
                        foreach (var val in inValues)
                        {
                            var inParamName = $"@p{index}_in{j}";
                            parameters[inParamName] = val;
                            inParams.Add(inParamName);
                            j++;
                        }
                        return $"{property} IN ({string.Join(", ", inParams)})";
                    }
                    parameters[paramName] = filter.Value;
                    return $"{property} IN ({paramName})";

                case FilterOperator.NotIn:
                    if (filter.Value is IEnumerable<object> notInValues)
                    {
                        var notInParams = new List<string>();
                        int k = 0;
                        foreach (var val in notInValues)
                        {
                            var notInParamName = $"@p{index}_nin{k}";
                            parameters[notInParamName] = val;
                            notInParams.Add(notInParamName);
                            k++;
                        }
                        return $"{property} NOT IN ({string.Join(", ", notInParams)})";
                    }
                    parameters[paramName] = filter.Value;
                    return $"{property} NOT IN ({paramName})";

                case FilterOperator.Between:
                    if (filter.Value is IList<object> betweenValues && betweenValues.Count == 2)
                    {
                        var fromParam = $"@p{index}_from";
                        var toParam = $"@p{index}_to";
                        parameters[fromParam] = betweenValues[0];
                        parameters[toParam] = betweenValues[1];
                        return $"{property} BETWEEN {fromParam} AND {toParam}";
                    }
                    return string.Empty;

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Build mệnh đề ORDER BY từ danh sách SortCondition (MySQL syntax).
        /// </summary>
        private string BuildOrderByClause(List<SortCondition> sorts)
        {
            if (sorts == null || sorts.Count == 0)
                return string.Empty;

            var orderParts = sorts.Select(s =>
                $"`{s.Property}` {(s.Desc ? "DESC" : "ASC")}"
            );

            return string.Join(", ", orderParts);
        }
        #endregion

        #region Methods query store procedure
        /// <summary>
        /// Thực hiện truy vấn dữ liệu từ database sử dụng stored procedure
        /// </summary>
        public async Task<List<T>> QueryUsingStoreProcedure<T>(Guid databaseID, string procedureName, Dictionary<string, object> param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = await GetDBConnectionAsync(databaseID);
                var dynamicParams = ConvertToDynamicParameters(param);
                var result = await cnn.QueryAsync<T>(sql: procedureName, param: dynamicParams, commandType: CommandType.StoredProcedure);
                return result.AsList();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        /// <summary>
        /// Thực hiện truy vấn dữ liệu từ database sử dụng stored procedure với connection có sẵn
        /// </summary>
        public async Task<List<T>> QueryUsingStoreProcedure<T>(IDbConnection cnn, string procedureName, Dictionary<string, object> param)
        {
            try
            {
                var dynamicParams = ConvertToDynamicParameters(param);
                var result = await cnn.QueryAsync<T>(sql: procedureName, param: dynamicParams, commandType: CommandType.StoredProcedure);
                return result.AsList();
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        /// <summary>
        /// Thực thi stored procedure (INSERT, UPDATE, DELETE)
        /// </summary>
        public async Task<bool> ExecuteUsingStoreProcedure(Guid databaseID, string procedureName, Dictionary<string, object> param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = await GetDBConnectionAsync(databaseID);
                var dynamicParams = ConvertToDynamicParameters(param);
                var rowsAffected = await cnn.ExecuteAsync(sql: procedureName, param: dynamicParams, commandType: CommandType.StoredProcedure);
                return rowsAffected > 0;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        /// <summary>
        /// Thực thi stored procedure (INSERT, UPDATE, DELETE) với connection có sẵn
        /// </summary>
        public async Task<bool> ExecuteUsingStoreProcedure(IDbConnection cnn, string procedureName, Dictionary<string, object> param)
        {
            try
            {
                var dynamicParams = ConvertToDynamicParameters(param);
                var rowsAffected = await cnn.ExecuteAsync(sql: procedureName, param: dynamicParams, commandType: CommandType.StoredProcedure);
                return rowsAffected > 0;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        /// <summary>
        /// Thực hiện truy vấn nhiều tập kết quả từ database sử dụng stored procedure
        /// </summary>
        public async Task<List<List<object>>> QueryMultipleUsingStoreProcedure(Guid databaseID, string procedureName, List<Type> types, Dictionary<string, object> param)
        {
            IDbConnection cnn = null;
            try
            {
                cnn = await GetDBConnectionAsync(databaseID);
                var dynamicParams = ConvertToDynamicParameters(param);
                var multi = await cnn.QueryMultipleAsync(sql: procedureName, param: dynamicParams, commandType: CommandType.StoredProcedure);
                return await ReadMultipleResults(multi, types);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }

        /// <summary>
        /// Thực hiện truy vấn nhiều tập kết quả từ database sử dụng stored procedure với connection có sẵn
        /// </summary>
        public async Task<List<List<object>>> QueryMultipleUsingStoreProcedure(IDbConnection cnn, string procedureName, List<Type> types, Dictionary<string, object> param)
        {
            try
            {
                var dynamicParams = ConvertToDynamicParameters(param);
                var multi = await cnn.QueryMultipleAsync(sql: procedureName, param: dynamicParams, commandType: CommandType.StoredProcedure);
                return await ReadMultipleResults(multi, types);
            }
            catch
            {
                throw;
            }
            finally
            {
                if (cnn != null && cnn.State == ConnectionState.Open)
                {
                    cnn.Close();
                    cnn.Dispose();
                }
            }
        }
        #endregion

        #region Methods get
        /// <summary>
        /// Lấy theo ID
        /// </summary>
        public async Task<object?> GetDataByID(Guid databaseID, Type modelType, string id, string columns = "*")
        {
            var param = new Dictionary<string, object>();
            var sql = GenerateSelectByID(param, modelType, id, columns);
            var result = await QueryUsingCommandText<object>(databaseID, sql, param);
            return result.Count > 0 ? result.First() : null;
        }

        /// <summary>
        /// Lấy theo ID
        /// </summary>
        public async Task<BaseModel> GetDataByID(Guid databaseID, Type modelType, string id)
        {
            var param = new Dictionary<string, object>();
            var sql = GenerateSelectByID(param, modelType, id);
            var result = await QueryUsingCommandText<BaseModel>(databaseID, sql, param);
            return result.Count > 0 ? result.First() : null;
        }

        /// <summary>
        /// Lấy bản ghi theo ID
        /// </summary>
        public async Task<T> GetDataByID<T>(Guid databaseID, string id) where T : BaseModel
        {
            var param = new Dictionary<string, object>();
            var sql = GenerateSelectByID(param, typeof(T), id);
            var result = await QueryUsingCommandText<T>(databaseID, sql, param);
            return result.Count > 0 ? result.First() : null;
        }
        #endregion

        #region Helper methods
        /// <summary>
        /// Tạo câu truy vấn SELECT theo ID với hỗ trợ Table attribute
        /// </summary>
        /// <param name="param">Dictionary chứa các tham số</param>
        /// <param name="modelType">Kiểu dữ liệu của model</param>
        /// <param name="id">Giá trị ID cần tìm</param>
        /// <returns>Câu truy vấn SQL</returns>
        public string GenerateSelectByID(Dictionary<string, object> param, Type modelType, string id, string columns = "*")
        {
            var sql = $"SELECT {columns} FROM {modelType.GetViewOrTableName()} WHERE {modelType.GetPrimaryKeyFieldName()} = @IDValue;";
            param = new Dictionary<string, object>()
            {
                { "IDValue",  GetSqlValue(id)}
            };
            return sql;
        }

        /// <summary>
        /// Sinh giá trị where dựa vào kiểu dữ liệu parameter
        /// </summary>
        /// <param name="value">Parameter</param>
        protected object GetSqlValue(string value)
        {
            Guid guidValue;
            if (Guid.TryParse(value, out guidValue))
            {
                return guidValue;
            }
            else
            {
                long longValue = 0;
                if (long.TryParse(value, out longValue))
                {
                    return longValue;
                }
                else
                {
                    return value;
                }
            }
        }

        /// <summary>
        /// Chuyển đổi Dictionary sang DynamicParameters
        /// </summary>
        public DynamicParameters ConvertToDynamicParameters(Dictionary<string, object> param)
        {
            var dynamicParams = new DynamicParameters();
            if (param != null)
            {
                foreach (var kvp in param)
                {
                    dynamicParams.Add(kvp.Key, kvp.Value);
                }
            }
            return dynamicParams;
        }

        /// <summary>
        /// Đọc nhiều tập kết quả từ GridReader theo danh sách các kiểu dữ liệu
        /// </summary>
        private async Task<List<List<object>>> ReadMultipleResults(SqlMapper.GridReader multi, List<Type> types)
        {
            var results = new List<List<object>>();
            foreach (var type in types)
            {
                var result = await multi.ReadAsync(type);
                results.Add(result.Cast<object>().ToList());
            }
            return results;
        }
        #endregion
    }
}
