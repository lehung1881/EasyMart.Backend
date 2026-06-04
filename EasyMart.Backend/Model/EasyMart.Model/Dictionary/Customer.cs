using BASE.Service.Core.Attribute;
using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using System.ComponentModel.DataAnnotations;

namespace EasyMart.Model.Dictionary
{
    /// <summary>
    /// Danh mục khách hàng
    /// </summary>
    [ConfigTable("di_customer", "")]
    public class Customer : BaseModel
    {
        /// <summary>
        /// Khóa chính
        /// </summary>
        [Key]
        public Guid CustomerID { get; set; }

        /// <summary>
        /// Mã khách hàng (unique)
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// Tên khách hàng
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// Loại khách hàng (0: Cá nhân, 1: Doanh nghiệp)
        /// </summary>
        public int CustomerType { get; set; } = 0;

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Mã số thuế (áp dụng cho doanh nghiệp)
        /// </summary>
        public string TaxCode { get; set; }

        /// <summary>
        /// Địa chỉ
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// 1: Đang sử dụng, 2: Ngừng sử dụng
        /// </summary>
        public RecordStatus Status { get; set; } = RecordStatus.Active;
    }
}
