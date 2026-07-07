using BASE.Service.Core.Attribute;
using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyMart.Model.Business
{
    /// <summary>
    /// Master của đơn hàng bán
    /// </summary>
    [ConfigTable("sa_order", "")]
    public class SAOrder : BaseModel
    {
        /// <summary>
        /// Khóa chính đơn hàng
        /// </summary>
        [Key]
        public Guid RefID { get; set; }

        /// <summary>
        /// Số đơn hàng
        /// </summary>
        [Required]
        public string RefNo { get; set; }

        /// <summary>
        /// Ngày lập đơn hàng
        /// </summary>
        [Required]
        public DateTime RefDate { get; set; }

        /// <summary>
        /// ID khách hàng
        /// </summary>
        public Guid? CustomerID { get; set; }

        /// <summary>
        /// Mã khách hàng snapshot
        /// </summary>
        public string CustomerCode { get; set; }

        /// <summary>
        /// Tên khách hàng snapshot
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// ID nhân viên thu ngân lập đơn
        /// </summary>
        public Guid? CashierID { get; set; }

        /// <summary>
        /// Tên nhân viên thu ngân lập đơn
        /// </summary>
        [Required]
        public string CashierName { get; set; }

        /// <summary>
        /// ID kho xuất hàng
        /// </summary>
        [Required]
        public Guid StockID { get; set; }

        /// <summary>
        /// Mã kho snapshot
        /// </summary>
        [Required]
        public string StockCode { get; set; }

        /// <summary>
        /// Tên kho snapshot
        /// </summary>
        [Required]
        public string StockName { get; set; }

        /// <summary>
        /// Tổng tiền hàng trước giảm giá và VAT
        /// </summary>
        public decimal SubTotalAmount { get; set; }

        /// <summary>
        /// Số tiền giảm giá trên tổng đơn
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Tiền VAT
        /// </summary>
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Tổng tiền phải thanh toán
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Số tiền khách đã trả
        /// </summary>
        public decimal PaidAmount { get; set; }

        /// <summary>
        /// Tiền thối lại cho khách
        /// </summary>
        public decimal ChangeAmount { get; set; }

        /// <summary>
        /// Hình thức thanh toán (1: Tiền mặt, 2: Chuyển khoản,...)
        /// </summary>
        [Required]
        public int PaymentMethod { get; set; } = 1;

        /// <summary>
        /// Trạng thái đơn
        /// </summary>
        [Required]
        public int OrderStatus { get; set; } = 1;

        /// <summary>
        /// Ghi chú đơn hàng
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Ngày tạo bản ghi
        /// </summary>
        public DateTime? CreatedDate { get; set; }

        /// <summary>
        /// Người tạo bản ghi
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Ngày sửa bản ghi gần nhất
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Người sửa bản ghi gần nhất
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Danh sách chi tiết đơn hàng
        /// </summary>
        [NotMapped]
        public List<SAOrderDetail> SAOrderDetails { get; set; }

        /// <summary>
        /// Khởi tạo và cấu hình Detail cho Master
        /// </summary>
        public SAOrder()
        {
            this.ModelDetailConfigs = new List<ModelDetailConfig>()
            {
                // mapping: tableName của detail, Khóa ngoại liên kết, Tên property chứa list ở Master
                new ModelDetailConfig("sa_order_detail", "RefID", "SAOrderDetails", true, true)
            };
        }
    }
}