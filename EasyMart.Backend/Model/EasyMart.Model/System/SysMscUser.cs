using BASE.Service.Core.Attribute;
using BASE.Service.Core.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace EasyMart.Model.System
{
    [ConfigTable("sys_msc_user", "")]
    public class SysMscUser : BaseModel
    {
        [Key]
        public Guid UserID { get; set; }

        /// <summary>
        /// 0: Hoạt động, 1: Ngừng hoạt động, 2: Chờ xác nhận Email
        /// </summary>
        public int Status { get; set; }

        public int Gender { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string MobilePhone { get; set; }

        public Guid? RoleID { get; set; }

        public string RoleCode { get; set; }

        public string RoleName { get; set; }

        public bool IsSystem { get; set; }
    }
}