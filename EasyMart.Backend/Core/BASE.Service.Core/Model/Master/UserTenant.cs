using BASE.Service.Core.Attribute;

namespace BASE.Service.Core.Model
{
    [ConfigTable("tenant_user", "")]
    public class UserTenant : BaseModel
    {
        /// <summary>
        /// ID người dùng (PK, FK → user.UserID)
        /// </summary>
        public Guid UserID { get; set; }

        /// <summary>
        /// ID khách hàng (PK, không FK - chấp nhận dư thừa)
        /// </summary>
        public Guid TenantID { get; set; }

        /// <summary>
        /// Thời điểm gán
        /// </summary>
        public DateTime AssignedAt { get; set; }
    }
}
