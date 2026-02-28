using BASE.Service.Core.Attribute;
using System.ComponentModel.DataAnnotations;

namespace BASE.Service.Core.Model
{
    [ConfigTable("permission", "")]
    public class Permission : BaseModel
    {
        /// <summary>
        /// Khóa chính
        /// </summary>
        [Key]
        public Guid PermissionID { get; set; }

        /// <summary>
        /// Tên quyền theo quy ước module:action
        /// Ví dụ: product:read, order:write, user:delete
        /// </summary>
        public string PermissionName { get; set; }

        /// <summary>
        /// Mô tả quyền
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Module tương ứng (Product, Order, User...)
        /// </summary>
        public string? Module { get; set; }
    }

    [ConfigTable("role_permission", "")]
    public class RolePermission : BaseModel
    {
        /// <summary>
        /// ID vai trò (PK, FK → msc_role.RoleID)
        /// </summary>
        public Guid RoleID { get; set; }

        /// <summary>
        /// ID quyền hạn (PK, FK → permission.PermissionID)
        /// </summary>
        public Guid PermissionID { get; set; }
    }
}
