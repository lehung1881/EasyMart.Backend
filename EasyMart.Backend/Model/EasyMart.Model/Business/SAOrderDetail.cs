using BASE.Service.Core.Attribute;
using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyMart.Model.Business
{
    /// <summary>
    /// Detail của đơn hàng bán
    /// </summary>
    [ConfigTable("sa_order_detail", "")]
    public class SAOrderDetail : BaseModel
    {
        /// <summary>
        /// Khóa chính dòng chi tiết
        /// </summary>
        [Key]
        public Guid RefDetailID { get; set; }

        /// <summary>
        /// Khóa ngoại về đơn hàng master
        /// </summary>
        public Guid RefID { get; set; }

        /// <summary>
        /// ID sản phẩm/hàng hóa
        /// </summary>
        public Guid InventoryItemID { get; set; }

        /// <summary>
        /// Mã sản phẩm snapshot tại thời điểm bán
        /// </summary>
        public string InventoryItemCode { get; set; }

        /// <summary>
        /// Tên sản phẩm snapshot tại thời điểm bán
        /// </summary>
        public string InventoryItemName { get; set; }

        /// <summary>
        /// Diễn giải chi tiết dòng
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// ID đơn vị tính
        /// </summary>
        public Guid? UnitID { get; set; }

        /// <summary>
        /// Tên đơn vị tính snapshot
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// ID đơn vị tính chính
        /// </summary>
        public Guid? MainUnitID { get; set; }

        /// <summary>
        /// Tên đơn vị tính chính snapshot
        /// </summary>
        public string MainUnitName { get; set; }

        /// <summary>
        /// Tỉ lệ quy đổi đơn vị tính
        /// </summary>
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// Phép tính quy đổi đơn vị tính ("*" hoặc "/")
        /// </summary>
        public string ExchangeRateOperator { get; set; } = "*";

        /// <summary>
        /// Số lượng bán
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Số lượng theo đơn vị tính chính
        /// </summary>
        public decimal MainQuantity { get; set; }

        /// <summary>
        /// Đơn giá bán
        /// </summary>
        public decimal UnitPrice { get; set; }

        /// <summary>
        /// Đơn giá theo đơn vị tính chính
        /// </summary>
        public decimal MainUnitPrice { get; set; }

        /// <summary>
        /// Tỷ lệ chiết khấu
        /// </summary>
        public decimal DiscountRate { get; set; }

        /// <summary>
        /// Số tiền giảm giá trên dòng
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Tỷ lệ VAT
        /// </summary>
        public decimal VatRate { get; set; }

        /// <summary>
        /// Tên mức VAT
        /// </summary>
        public string VatRateName { get; set; }

        /// <summary>
        /// Số tiền VAT
        /// </summary>
        public decimal VatAmount { get; set; }

        /// <summary>
        /// Thành tiền dòng
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Số thứ tự dòng
        /// </summary>
        public int SortOrder { get; set; } = 1;
    }
}