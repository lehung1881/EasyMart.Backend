using BASE.Service.Core.Attribute;
using BASE.Service.Core.Model;
using System.ComponentModel.DataAnnotations;

namespace EasyMart.Model.Dictionary
{
    /// <summary>
    /// Model danh mục Hàng hóa (Vật tư hàng hóa)
    /// </summary>
    [ConfigTable("di_inventory_item", "")]
    public class InventoryItem : BaseModel
    {
        /// <summary>
        /// Khóa chính (Primary Key)
        /// </summary>
        [Key]
        public Guid InventoryItemID { get; set; }

        /// <summary>
        /// Mã hàng hóa (SKU) - Duy nhất, dùng để quét mã vạch hoặc tìm kiếm nhanh
        /// </summary>
        public string InventoryItemCode { get; set; }

        /// <summary>
        /// Tên hàng hóa
        /// </summary>
        public string InventoryItemName { get; set; }

        /// <summary>
        /// Loại hàng hóa (0: Hàng hóa, 1: Dịch vụ, 2: Nguyên vật liệu...)
        /// </summary>
        public int InventoryItemType { get; set; } = 0;

        /// <summary>
        /// ID đơn vị tính chính (Liên kết tới di_unit)
        /// </summary>
        public Guid? UnitID { get; set; }

        /// <summary>
        /// Tên đơn vị tính (Lưu vết để hiển thị nhanh không cần join bảng Unit)
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// Giá mua ngầm định (Sử dụng DECIMAL(20,4))
        /// </summary>
        public decimal BuyPrice { get; set; } = 0;

        /// <summary>
        /// Giá bán niêm yết (Sử dụng DECIMAL(20,4))
        /// </summary>
        public decimal SellPrice { get; set; } = 0;

        /// <summary>
        /// ID kho ngầm định (Liên kết tới di_stock)
        /// </summary>
        public Guid? StockID { get; set; }

        /// <summary>
        /// Định mức tồn kho tối thiểu để cảnh báo nhập hàng
        /// </summary>
        public decimal MinimumStock { get; set; } = 0;

        /// <summary>
        /// Danh sách đường dẫn ảnh sản phẩm (Lưu dạng chuỗi JSON hoặc CSV)
        /// </summary>
        public string Images { get; set; }

        /// <summary>
        /// Trạng thái hoạt động (1: Đang sử dụng, 2: Ngừng sử dụng)
        /// </summary>
        public int Status { get; set; } = 1;

        /// <summary>
        /// Ghi chú, mô tả chi tiết sản phẩm
        /// </summary>
        public string Description { get; set; }
    }
}
