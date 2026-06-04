using BASE.Service.Core.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BASE.Service.Core.Enum;
using System.Text.Json.Serialization;

namespace BASE.Service.Core.Model
{
    public class BaseModel
    {

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Người tạo
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Ngày sửa
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Người sửa
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Trạng thái của model
        /// </summary>
        [NotMapped]
        public ModelState ModelState { get; set; } = ModelState.Insert;

        /// <summary>
        /// Danh sách cột update
        /// </summary>
        [NotMapped]
        public List<string> UpdateColumns { get; set; }

        /// <summary>
        /// Cấu hình detail
        /// </summary>
        [NotMapped]
        [JsonIgnore]
        public List<ModelDetailConfig> ModelDetailConfigs { get; set; }

        #region Method
        /// <summary>
        /// Set giá trị
        /// </summary>
        /// <param name="value"></param>
        public void SetPrimaryKey(string value)
        {
            PropertyInfo[] props = this.GetType().GetProperties();

            PropertyInfo propertyInfoKey = null;
            if (props != null)
            {
                propertyInfoKey = props.SingleOrDefault(
                    p => p.GetCustomAttribute<KeyAttribute>(true) != null
                );

                if (propertyInfoKey != null)
                {
                    if (propertyInfoKey.PropertyType == typeof(long))
                        propertyInfoKey.SetValue(this, long.Parse(value));
                    else if (propertyInfoKey.PropertyType == typeof(int))
                        propertyInfoKey.SetValue(this, int.Parse(value));
                    else if (propertyInfoKey.PropertyType == typeof(Guid))
                        propertyInfoKey.SetValue(this, Guid.Parse(value));
                    else
                        propertyInfoKey.SetValue(this, value);
                }
            }
        }


        /// <summary>
        /// Lấy tên bảng trong Database
        /// </summary>
        /// <returns></returns>
        public string GetViewOrTableName()
        {
            var tableAttr = (ConfigTable)GetType().GetCustomAttributes(typeof(ConfigTable), false).FirstOrDefault();
            string viewOrTabble = !string.IsNullOrEmpty(tableAttr.ViewName) ? tableAttr.ViewName : tableAttr.TableName;
            return viewOrTabble;
        }

        /// <summary>
        /// Lấy tên bảng trong Database
        /// </summary>
        /// <returns></returns>
        public string GetTableName()
        {
            var tableAttr = (ConfigTable)GetType().GetCustomAttributes(typeof(ConfigTable), false).FirstOrDefault();
            return tableAttr.TableName;
        }

        /// <summary>
        /// Lấy ra khóa chính
        /// </summary>
        /// <returns></returns>
        public string GetPrimaykeyField()
        {
            var properties = this.GetType().GetProperties();
            var keyProperty = properties.FirstOrDefault(p => p.GetCustomAttribute<KeyAttribute>() != null);
            return keyProperty?.Name;
        }

        /// <summary>
        /// Lấy giá trị của khóa chính
        /// </summary>
        /// <returns></returns>
        public object GetPrimaryKeyValue()
        {
            return this.GetValueByAttribute(typeof(KeyAttribute));
        }

        /// <summary>
        /// Kiểm tra null khóa chính
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsNullOrEmptyPrimary()
        {
            var value = GetPrimaryKeyValue();

            if (value is null)
                return true;

            return value switch
            {
                Guid g => g == Guid.Empty,
                string s => string.IsNullOrWhiteSpace(s),
                int i => i == 0,
                long l => l == 0L,
                _ => false
            };
        }

        /// <summary>
        /// Lấy ra filed theo Attribute
        /// </summary>
        /// <param name="typeAttr"></param>
        /// <returns></returns>
        public string GetFieldNameByAttribute(Type typeAttr)
        {
            PropertyInfo[] props = this.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0).ToArray();
            var customProperty = props.FirstOrDefault(p => p.GetCustomAttribute(typeAttr, true) != null);
            return customProperty?.Name;
        }

        /// <summary>
        /// Lấy giá trị của một field bằng thuộc tính
        /// </summary>
        /// <param name="typeAttr"></param>
        /// <returns></returns>
        public object GetValueByAttribute(Type typeAttr)
        {
            PropertyInfo[] props = this.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0).ToArray();
            PropertyInfo oProp = null;
            if (props != null)
            {
                oProp = props.SingleOrDefault(p => p.GetCustomAttribute(typeAttr, true) != null);
            }
            if (oProp != null)
            {
                return oProp.GetValue(this);
            }
            return null;
        }

        /// <summary>
        /// Clone instance
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return MemberwiseClone();
        }

        /// <summary>
        /// Set giá trị khóa chính mặc định
        /// </summary>
        public void SetAutoPrimaryKey()
        {
            SetValueAutoPrimaryKey();
        }

        /// <summary>
        /// Set giá trị khóa chính mặc định
        /// </summary>
        internal protected virtual void SetValueAutoPrimaryKey()
        {
            PropertyInfo[] props = this.GetType().GetProperties();
            PropertyInfo propertyInfoKey = null;
            if (props != null)
            {
                propertyInfoKey = props.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);
                if (propertyInfoKey != null)
                {
                    if (propertyInfoKey.PropertyType == typeof(long))
                    {
                        // Int thường để tự tăng nên không cần set
                    }
                    else if (propertyInfoKey.PropertyType == typeof(Int32))
                    {
                        // Int thường để tự tăng nên không cần set
                    }
                    else if (propertyInfoKey.PropertyType == typeof(Guid))
                    {
                        propertyInfoKey.SetValue(this, Guid.NewGuid()); // Nếu là GUID thì tự sinh NewGuid()
                    }
                    else
                    {
                        // String thường đã có giá trị nên không cần set
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Cấu hình chi tiết detail
    /// </summary>
    public class ModelDetailConfig
    {
        /// <summary>
        /// Tên bảng detail trong DB
        /// </summary>
        public string DetailTableName { get; set; }
        /// <summary>  
        /// Tên cột ForeignKey  
        /// </summary>  
        public string ForeignKeyName { get; set; }

        /// <summary>  
        /// Tên property kiểu List of detailObject trên master model  
        /// </summary>  
        public string PropertyOnMasterModel { get; set; }

        /// <summary>  
        /// Có xóa trước khi xóa master model không  
        /// </summary>  
        public bool CascadeOnDeleteMasterModel { get; set; }

        /// <summary>
        /// Xóa detail trước khi insert/update lại
        /// </summary>
        public bool DeleteBeforeUpsert { get; set; } = false;

        public ModelDetailConfig(string detailTableName, string foreignKeyName, string propertyOnMasterModel, bool cascadeOnDeleteMasterModel, bool deleteBeforeUpsert)
        {
            this.DetailTableName = detailTableName;
            this.ForeignKeyName = foreignKeyName;
            this.PropertyOnMasterModel = propertyOnMasterModel;
            this.CascadeOnDeleteMasterModel = cascadeOnDeleteMasterModel;
            this.DeleteBeforeUpsert = deleteBeforeUpsert;
        }
    }
}
