using BASE.Service.Core.Attribute;
using BASE.Service.Core.Model;
using BASE.Service.Core.Utils;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyMart.Model.System
{
    [ConfigTable("sys_msc_role_permission_mapping", "")]
    public class SysMscRolePermissionMapping : BaseModel
    {
        [Key]
        public Guid ID { get; set; }

        public Guid RoleID { get; set; }

        /// <summary>
        /// Mã màn hình/chức năng
        /// </summary>
        public string SubSystemCode { get; set; }

        /// <summary>
        /// Danh sách quyền dạng JSON
        /// Ví dụ: ["View","Add","Edit","Delete"]
        /// </summary>
        public string ListPermission { get; set; }

        /// <summary>
        /// List phân quyền Object
        /// </summary>
        [NotMapped]
        public object ListPermissionObject
        {
            get => ConvertUtil.DeserializeObject<Dictionary<string, bool>>(ListPermission);
            set => ListPermission = ConvertUtil.SerializeObject(value);
        }
    }
}