using BASE.Service.Core.Attribute;
using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using System.ComponentModel.DataAnnotations;

namespace EasyMart.Model.Dictionary
{
    /// <summary>
    /// Bảng mapping đơn vị chuyển đổi với danh mục VTHH
    /// </summary>
    [ConfigTable("di_inventory_item_unit_convert", "")]
    public class InventoryItemUnitConvert : BaseModel
    {
        /// <summary>
        /// Khóa chính
        /// </summary>
        [Key]
        public Guid UnitConvertID { get; set; }

        /// <summary>
        /// Khóa ngoại của bảng inventory_item
        /// </summary>
        public Guid InventoryItemID { get; set; }

        /// <summary>
        /// ID của đơn vị tính
        /// </summary>
        public Guid UnitID { get; set; }

        /// <summary>
        /// Tên của đơn vị tính
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// Số thứ tự đơn vị chuyển đổi
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// Tỉ lệ chuyển đổi (phải > 0)
        /// </summary>
        public decimal ConvertRate { get; set; }

        /// <summary>
        /// Phép tính (1: Phép nhân, 2: Phép chia)
        /// </summary>
        public int ExchangeRateOperator { get; set; } = 1;

        /// <summary>
        /// Tên toán tử chuyển đổi
        /// </summary>
        public string ExchangeRateOperatorText { get; set; } = "Phép nhân";

        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }
    }
}
