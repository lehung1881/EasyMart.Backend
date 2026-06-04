using BASE.Service.Core.Attribute;
using BASE.Service.Core.Utils;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace BASE.Service.Core
{
    public static class ExtensionUtility
    {
        /// <summary>
        /// Lấy về tên cột khóa chính
        /// </summary>
        /// <returns>Trả về tên key của object</returns>
        public static string GetPrimaryKeyFieldName(this Type type)
        {
            return type.GetFieldName(typeof(KeyAttribute));
        }

        /// <summary>
        /// Lấy về tên cột theo attrType
        /// </summary>
        /// <returns>Trả về tên key của object</returns>
        public static string GetFieldName(this Type type, Type attrType)
        {
            string primaryKeyName = string.Empty;
            PropertyInfo[] props = type.GetProperties();
            if (props != null)
            {
                var propertyInfoKey = props.SingleOrDefault(p => p.GetCustomAttribute(attrType, true) != null);
                if (propertyInfoKey != null)
                {
                    primaryKeyName = propertyInfoKey.Name;
                }
            }
            return primaryKeyName;
        }

        /// <summary>
        /// Lấy tên bảng mapping trong database
        /// </summary>
        public static string GetViewOrTableName(this Type type)
        {
            var configTable = ((ConfigTable)type.GetCustomAttributes(typeof(ConfigTable), true).FirstOrDefault());
            string tableName = configTable?.ViewName;
            if (string.IsNullOrEmpty(tableName))
            {
                tableName = configTable?.TableName;
            }
            return tableName;
        }

        public static T GetObject<T>(this Dictionary<string, object> dic, string key)
        {
            T value = default(T);
            if (dic.ContainsKey(key) && dic[key] != null)
            {
                value = ConvertUtil.DeserializeObject<T>(ConvertUtil.SerializeObject(dic[key]));
            }

            return value;
        }

        /// <summary>
        /// Converter obj sang dạng dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ToDictionary(this object obj)
        {
            if (obj == null)
            {
                return new Dictionary<string, object>();
            }
            else
            {
                return ConvertUtil.DeserializeObject<Dictionary<string, object>>(ConvertUtil.SerializeObject(obj));
            }
        }

        /// <summary>
        /// Get giá trị của một thuộc tính
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objEntity"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T GetValue<T>(this object objEntity, string propertyName)
        {
            T value = default(T);
            if (objEntity != null && !string.IsNullOrEmpty(propertyName))
            {
                if (objEntity.GetType() == typeof(Dictionary<string, object>))
                {
                    return (objEntity as Dictionary<string, object>).GetObject<T>(propertyName);
                }
                else
                {
                    PropertyInfo info = objEntity.GetType().GetProperty(propertyName);
                    if (info != null)
                    {
                        object objValue = info.GetValue(objEntity);
                        if (objValue != null)
                        {
                            value = (T)objValue;
                        }
                    }
                    else
                    {
                        return objEntity.ToDictionary().GetObject<T>(propertyName);
                    }
                }
            }
            return value;
        }

        /// <summary>
        /// Hàm gán giá trị cho thuộc tính của đối tượng
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu trả về</typeparam>
        /// <param name="objEntity">Đối tượng có thuộc tính cần lấy</param>
        /// <param name="propertyName">Tên thuộc tính cần lấy</param>
        public static void SetValue(this object objEntity, string propertyName, object value)
        {
            PropertyInfo propertyInfo = objEntity.GetType().GetProperty(propertyName, BindingFlags.SetProperty | BindingFlags.IgnoreCase
                | BindingFlags.Public | BindingFlags.Instance);
            if (propertyInfo != null)
            {
                Type type = propertyInfo.PropertyType;
                if ((!object.Equals(value, DBNull.Value)) && propertyInfo.CanWrite)
                {
                    if (value != null)
                    {
                        propertyInfo.SetValue(objEntity, Convert.ChangeType(value, Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType), null);
                    }
                    else
                    {
                        propertyInfo.SetValue(objEntity, null, null);
                    }
                }
            }
        }

    }
}
