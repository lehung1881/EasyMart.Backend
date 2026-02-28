using BASE.Service.Core.Attribute;
using System.ComponentModel.DataAnnotations;

namespace BASE.Service.Core.Model
{
    [ConfigTable("tenant", "")]
    public class Tenant : BaseModel
    {
        /// <summary>
        /// Khóa chính
        /// </summary>
        [Key]
        public Guid TenantID { get; set; }

        /// <summary>
        /// Mã định danh khách hàng (vd: CUST_001)
        /// </summary>
        public string TenantCode { get; set; }

        /// <summary>
        /// Tên khách hàng
        /// </summary>
        public string TenantName { get; set; }

        /// <summary>
        /// Email liên hệ
        /// </summary>
        public string? ContactEmail { get; set; }

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        public string? ContactPhone { get; set; }

        /// <summary>
        /// Trạng thái hoạt động
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Ngày hết hạn license
        /// </summary>
        public DateTime? ExpiredDate { get; set; }

        /// <summary>
        /// Thời điểm tạo
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Thời điểm cập nhật
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete
        /// </summary>
        public bool IsDeleted { get; set; } = false;
    }
}
