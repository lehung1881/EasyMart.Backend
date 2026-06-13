using BASE.Service.Core.Attribute;
using BASE.Service.Core.Model;
using System;
using System.ComponentModel.DataAnnotations;

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

        public List<SysMscRolePermissionMapping> SysMscRolePermissionMapping { get; set; }
    }
}