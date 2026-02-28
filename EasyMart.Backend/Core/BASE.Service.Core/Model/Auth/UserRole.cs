using BASE.Service.Core.Attribute;

namespace BASE.Service.Core.Model
{
    [ConfigTable("user_role", "")]
    public class UserRole : BaseModel
    {
        /// <summary>
        /// ID người dùng (PK, FK → user.UserID)
        /// </summary>
        public Guid UserID { get; set; }

        /// <summary>
        /// ID vai trò (PK, FK → msc_role.RoleID)
        /// </summary>
        public Guid RoleID { get; set; }

        /// <summary>
        /// Thời điểm gán
        /// </summary>
        public DateTime AssignedAt { get; set; }

        /// <summary>
        /// Người thực hiện gán
        /// </summary>
        public Guid? AssignedBy { get; set; }
    }
}
