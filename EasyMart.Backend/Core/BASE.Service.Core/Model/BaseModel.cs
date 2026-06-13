
using BASE.Service.Core.Attribute;
using BASE.Service.Core.Enum;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
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

        #region Table helpers

        /// <summary>
        /// Gán giá trị cho khóa chính của model.
        /// Tự động chuyển đổi sang đúng kiểu dữ liệu.
        /// </summary>
        public void SetPrimaryKey(string value)
        {
            PropertyInfo[] props = this.GetType().GetProperties();
            PropertyInfo propertyInfoKey = null;

            if (props != null)
            {
                propertyInfoKey = props.SingleOrDefault(p => p.GetCustomAttribute<KeyAttribute>(true) != null);

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
        /// Lấy tên View hoặc Table được cấu hình.
        /// Ưu tiên ViewName nếu được khai báo.
        /// </summary>
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

            if (value is null) return true;

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
                        propertyInfoKey.SetValue(this, Guid.NewGuid());
                    }
                    else
                    {
                        // String thường đã có giá trị nên không cần set
                    }
                }
            }
        }

        #endregion

        #region Validate (Insert: full | Update: only UpdateColumns)

        private static readonly ConcurrentDictionary<Type, PropertyMeta[]> _validateMetaCache = new();

        /// <summary>
        /// Metadata phục vụ validate property.
        /// Lưu thông tin validator và trạng thái mapping.
        /// </summary>
        private sealed class PropertyMeta
        {
            public PropertyInfo Prop { get; init; }
            public ValidationAttribute[] Validators { get; init; }
            public bool IsNotMapped { get; init; }
        }

        /// <summary>
        /// Validate dữ liệu theo DataAnnotations.
        /// Hỗ trợ Insert và Update theo UpdateColumns.
        /// </summary>
        public List<ValidateResult> ValidateModel(bool ignoreNotMapped = true)
        {
            var errors = new List<ValidateResult>();
            var type = GetType();
            var metas = _validateMetaCache.GetOrAdd(type, BuildValidateMetas);

            var recordId = GetPrimaryKeyValue();

            HashSet<string> updateCols = null;

            if (ModelState == ModelState.Update)
            {
                if (UpdateColumns == null || UpdateColumns.Count == 0)
                {
                    return errors;
                }

                updateCols = new HashSet<string>(
                    UpdateColumns.Where(x => !string.IsNullOrWhiteSpace(x)),
                    StringComparer.OrdinalIgnoreCase);
            }

            foreach (var meta in metas)
            {
                if (ignoreNotMapped && meta.IsNotMapped)
                    continue;

                if (meta.Validators == null || meta.Validators.Length == 0)
                    continue;

                if (updateCols != null && !updateCols.Contains(meta.Prop.Name))
                    continue;

                var value = meta.Prop.GetValue(this);

                foreach (var validator in meta.Validators)
                {
                    var context = new ValidationContext(this)
                    {
                        MemberName = meta.Prop.Name
                    };

                    var vr = validator.GetValidationResult(value, context);

                    if (vr == ValidationResult.Success)
                        continue;

                    errors.Add(new ValidateResult
                    {
                        ID = recordId,
                        Code = MapValidateCode(validator),
                        ErrorMessage = vr?.ErrorMessage ?? $"{meta.Prop.Name} không hợp lệ.",
                        AdditionInfo = BuildAdditionInfo(meta.Prop, validator, value)
                    });
                }
            }

            return errors;
        }

        /// <summary>
        /// Xây dựng metadata phục vụ validate model.
        /// Thu thập validator và trạng thái mapping của các property.
        /// </summary>
        private static PropertyMeta[] BuildValidateMetas(Type type)
        {
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetIndexParameters().Length == 0)
                .ToArray();

            var metas = new List<PropertyMeta>(props.Length);

            foreach (var p in props)
            {
                var validators = p.GetCustomAttributes<ValidationAttribute>(true).ToArray();
                var notMapped = p.GetCustomAttribute<NotMappedAttribute>(true) != null;

                metas.Add(new PropertyMeta
                {
                    Prop = p,
                    Validators = validators,
                    IsNotMapped = notMapped
                });
            }

            return metas.ToArray();
        }

        private static string MapValidateCode(ValidationAttribute attr)
        {
            return attr switch
            {
                RequiredAttribute => "VALIDATE_REQUIRED",
                MaxLengthAttribute => "VALIDATE_MAX_LENGTH",
                MinLengthAttribute => "VALIDATE_MIN_LENGTH",
                StringLengthAttribute => "VALIDATE_STRING_LENGTH",
                RangeAttribute => "VALIDATE_RANGE",
                RegularExpressionAttribute => "VALIDATE_REGEX",
                EmailAddressAttribute => "VALIDATE_EMAIL",
                PhoneAttribute => "VALIDATE_PHONE",
                _ => "VALIDATE_INVALID"
            };
        }

        private static object BuildAdditionInfo(PropertyInfo prop, ValidationAttribute attr, object attemptedValue)
        {
            if (attr is RequiredAttribute)
            {
                return new
                {
                    Field = prop.Name,
                    Rule = "Required",
                    AttemptedValue = attemptedValue
                };
            }

            if (attr is MaxLengthAttribute maxLen)
            {
                return new
                {
                    Field = prop.Name,
                    Rule = "MaxLength",
                    MaxLength = maxLen.Length,
                    AttemptedValue = attemptedValue
                };
            }

            if (attr is MinLengthAttribute minLen)
            {
                return new
                {
                    Field = prop.Name,
                    Rule = "MinLength",
                    MinLength = minLen.Length,
                    AttemptedValue = attemptedValue
                };
            }

            if (attr is StringLengthAttribute strLen)
            {
                return new
                {
                    Field = prop.Name,
                    Rule = "StringLength",
                    MinimumLength = strLen.MinimumLength,
                    MaximumLength = strLen.MaximumLength,
                    AttemptedValue = attemptedValue
                };
            }

            if (attr is RangeAttribute range)
            {
                return new
                {
                    Field = prop.Name,
                    Rule = "Range",
                    Minimum = range.Minimum,
                    Maximum = range.Maximum,
                    AttemptedValue = attemptedValue
                };
            }

            if (attr is RegularExpressionAttribute regex)
            {
                return new
                {
                    Field = prop.Name,
                    Rule = "Regex",
                    Pattern = regex.Pattern,
                    AttemptedValue = attemptedValue
                };
            }

            return new
            {
                Field = prop.Name,
                Rule = attr.GetType().Name,
                AttemptedValue = attemptedValue
            };
        }

        #endregion
    }

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