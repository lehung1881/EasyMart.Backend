using BASE.Service.Core.Attribute;
using BASE.Service.Core.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyMart.Model.System
{
    [ConfigTable("sys_msc_role", "")]
    public class SysMscRole : BaseModel
    {
        [Key]
        public Guid RoleID { get; set; }

        public string RoleCode { get; set; }

        public string RoleName { get; set; }

        public string Description { get; set; }

        public bool IsSystem { get; set; }

        [NotMapped]
        public List<SysMscRolePermissionMapping> SysMscRolePermissionMapping { get; set; }

        /// <summary>
        /// Cấu hình Detail
        /// </summary>
        public SysMscRole()
        {
            this.ModelDetailConfigs = new List<ModelDetailConfig>()
            {
                new ModelDetailConfig("sys_msc_role_permission_mapping", "RoleID", "SysMscRolePermissionMapping", true, true)
            };
        }
    }
}