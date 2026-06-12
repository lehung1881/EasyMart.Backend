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
        /// 0: Hoạt động, 1: Ngừng hoạt động
        /// </summary>
        public bool Inactive { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public string MobilePhone { get; set; }
    }
}