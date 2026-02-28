using BASE.Service.Core.Attribute;
using System.ComponentModel.DataAnnotations;

namespace BASE.Service.Core.Model
{
    [ConfigTable("msc_role", "")]
    public class MSCRole : BaseModel
    {
        /// <summary>
        /// Khóa chính
        /// </summary>
        [Key]
        public Guid RoleID { get; set; }

        /// <summary>
        /// Tên vai trò (Admin, Manager, User)
        /// </summary>
        public string RoleName { get; set; }

        /// <summary>
        /// Mô tả vai trò
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Thời điểm tạo
        /// </summary>
        public DateTime CreatedDate { get; set; }
    }
}
