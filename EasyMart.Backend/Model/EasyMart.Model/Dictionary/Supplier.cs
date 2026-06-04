using BASE.Service.Core.Attribute;
using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using System.ComponentModel.DataAnnotations;

namespace EasyMart.Model.Dictionary
{
    /// <summary>
    /// Danh mục nhà cung cấp
    /// </summary>
    [ConfigTable("di_supplier", "")]
    public class Supplier : BaseModel
    {
        /// <summary>
        /// Khóa chính
        /// </summary>
        [Key]
        public Guid SupplierID { get; set; }

        /// <summary>
        /// Mã nhà cung cấp (unique)
        /// </summary>
        public string SupplierCode { get; set; }

        /// <summary>
        /// Tên nhà cung cấp
        /// </summary>
        public string SupplierName { get; set; }

        /// <summary>
        /// Loại nhà cung cấp (0: Cá nhân, 1: Doanh nghiệp)
        /// </summary>
        public int SupplierType { get; set; } = 0;

        /// <summary>
        /// Số điện thoại
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Mã số thuế
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
