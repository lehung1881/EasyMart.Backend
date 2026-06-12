using BASE.Service.Core.Attribute;
using BASE.Service.Core.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace EasyMart.Model.System
{
    [ConfigTable("sys_msc_user_role", "")]
    public class SysMscUserRole : BaseModel
    {
        [Key]
        public Guid UserRoleID { get; set; }

        public Guid UserID { get; set; }

        public Guid RoleID { get; set; }
    }
}