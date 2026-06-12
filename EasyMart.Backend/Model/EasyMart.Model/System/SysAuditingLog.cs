using BASE.Service.Core.Attribute;
using BASE.Service.Core.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace EasyMart.Model.System
{
    /// <summary>
    /// Nhật ký thao tác hệ thống
    /// </summary>
    [ConfigTable("sys_auditing_log", "")]
    public class SysAuditingLog : BaseModel
    {
        /// <summary>
        /// Khóa chính của bảng
        /// </summary>
        [Key]
        public Guid ID { get; set; }

        /// <summary>
        /// Tham chiếu
        /// </summary>
        public string Reference { get; set; }

        /// <summary>
        /// Mô tả chi tiết hành động
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Loại hành động
        /// </summary>
        public int? ActionType { get; set; }

        /// <summary>
        /// Tên hành động
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// UserID của người thực hiện hành động
        /// </summary>
        public Guid UserID { get; set; }

        /// <summary>
        /// Tên người dùng thực hiện hành động
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// ID của đối tượng được thao tác
        /// </summary>
        public Guid? ModelID { get; set; }

        /// <summary>
        /// Tên đối tượng được thao tác
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// Địa chỉ IP của người dùng
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// ID công ty (Tenant)
        /// </summary>
        public Guid TenantID { get; set; }
    }
}