using BASE.Service.Core.Attribute;
using BASE.Service.Core.Model;
using System.ComponentModel.DataAnnotations;

namespace EasyMart.Model.Dictionary
{
    [ConfigTable("di_inventory_item", "")]
    public class InventoryItem : BaseModel
    {
        /// <summary>
        /// Khóa chính của bảng
        /// </summary>
        [Key]
        public Guid InventoryItemID { get; set; }
        
        /// <summary>
        /// Mã VTHH
        /// </summary>
        public string InventoryItemCode { get; set; }

        /// <summary>
        /// Tên VTHH
        /// </summary>
        public string InventoryItemName { get; set; }

        /// <summary>
        /// Phương pháp xuất kho ngầm định là FIFO
        /// </summary>
        public int ReleaseMethod { get; set; } = 1;

        /// <summary>
        /// Đơn vị tính chính
        /// </summary>
        public Guid? UnitID { get; set; }

        /// <summary>
        /// Tên đơn vị tính
        /// </summary>
        public string UnitName { get; set; }


        /// <summary>
        /// Loại VTHH: 0- hàng hóa, 1- nguyên vật liệu, 2- thành phẩm
        /// </summary>
        public int InventoryItemType { get; set; } = 0;

        /// <summary>
        /// Danh sách id nhóm VTHH
        /// </summary>
        public string InventoryItemCategoryIDList { get; set; }

        /// <summary>
        /// Danh sách Mã nhóm VTHH
        /// </summary>
        public string InventoryItemCategoryCodeList { get; set; }

        /// <summary>
        /// Danh sách tên nhóm VTHH
        /// </summary>
        public string InventoryItemCategoryNameList { get; set; }

        /// <summary>
        /// Số lượng tồn kho tối đa
        /// </summary>
        public decimal MaximumStock { get; set; } = 0;

        /// <summary>
        /// Số lượng tồn kho tối thiểu
        /// </summary>
        public decimal MinimumStock { get; set; } = 0;

        /// <summary>
        /// Số lượng tồn kho
        /// </summary>
        public decimal QuantityBalance { get; set; } = 0;

        /// <summary>
        /// Đơn vị thời gian bảo hành (3: năm, 2: tháng, 1: ngày)
        /// </summary>
        public int? WarrantyTimeUnit { get; set; }

        /// <summary>
        /// Nguồn gốc
        /// </summary>
        public string InventoryItemSource { get; set; }

        /// <summary>
        /// Mô tả
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ảnh vật tư hàng hóa (JSON)
        /// </summary>
        public string Images { get; set; }

        /// <summary>
        /// Trạng thái hoạt động: true-không hoạt động
        /// </summary>
        public bool Inactive { get; set; } = false;

        /// <summary>
        /// Đơn vị chuyển đổi (JSON)
        /// </summary>
        public string UnitList { get; set; }

        /// <summary>
        /// Giá mua
        /// </summary>
        public decimal BuyPrice { get; set; } = 0;

        /// <summary>
        /// Giá bán
        /// </summary>
        public decimal SellPrice { get; set; } = 0;

        /// <summary>
        /// Công thức backend
        /// </summary>
        public string BackEndFormula { get; set; }

        /// <summary>
        /// Công thức frontend
        /// </summary>
        public string FrontEndFormula { get; set; }

        /// <summary>
        /// Trường mở rộng 1
        /// </summary>
        public string DICustomField1 { get; set; }

        /// <summary>
        /// Trường mở rộng 2
        /// </summary>
        public string DICustomField2 { get; set; }

        /// <summary>
        /// Trường mở rộng 3
        /// </summary>
        public string DICustomField3 { get; set; }

        /// <summary>
        /// Trường mở rộng 4
        /// </summary>
        public string DICustomField4 { get; set; }

        /// <summary>
        /// Trường mở rộng 5
        /// </summary>
        public string DICustomField5 { get; set; }

        /// <summary>
        /// Trường mở rộng 6
        /// </summary>
        public string DICustomField6 { get; set; }

        /// <summary>
        /// Trường mở rộng 7
        /// </summary>
        public string DICustomField7 { get; set; }

        /// <summary>
        /// Trường mở rộng 8
        /// </summary>
        public string DICustomField8 { get; set; }

        /// <summary>
        /// Trường mở rộng 9
        /// </summary>
        public string DICustomField9 { get; set; }

        /// <summary>
        /// Trường mở rộng 10
        /// </summary>
        public string DICustomField10 { get; set; }

        /// <summary>
        /// Ngày tạo
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        public string CreatedBy { get; set; }

        /// <summary>
        /// Ngày sửa
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        public string ModifiedBy { get; set; }

        /// <summary>
        /// Vật tư hàng hóa có theo dõi theo mã quy cách không
        /// </summary>
        public bool IsFollowSerialNumber { get; set; } = false;

        /// <summary>
        /// True: nếu tất cả mã quy cách cho trùng, ngược lại false
        /// </summary>
        public bool IsAllowDuplicateSerialNumber { get; set; } = false;

        /// <summary>
        /// Thời gian bảo hành
        /// </summary>
        public decimal? WarrantyTime { get; set; }

        /// <summary>
        /// Cột dựa trên cách tính
        /// </summary>
        public int? BaseOnFormula { get; set; }

        /// <summary>
        /// Số lượng tồn khả dụng
        /// </summary>
        public decimal QuantityAvailable { get; set; } = 0;

        /// <summary>
        /// Thông tin combo vật tư dưới dạng JSON
        /// </summary>
        public string InventoryCombo { get; set; }
    }
}
