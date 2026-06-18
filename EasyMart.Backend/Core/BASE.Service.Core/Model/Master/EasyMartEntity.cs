using BASE.Service.Core.Attribute;
using System.ComponentModel.DataAnnotations;

namespace BASE.Service.Core.Model
{
    /// <summary>
    /// Model thực thể đại diện cho thông tin Siêu thị / Đơn vị thành viên (EasyMart).
    /// Ánh xạ trực tiếp với bảng "easymart" trong cơ sở dữ liệu.
    /// </summary>
    [ConfigTable("easymart", "")]
    public class EasyMartEntity : BaseModel
    {
        /// <summary>
        /// Khóa chính định danh Siêu thị
        /// </summary>
        [Key]
        public Guid EasyMartID { get; set; }

        /// <summary>
        /// Mã định danh siêu thị (Ví dụ: EM_HN01)
        /// </summary>
        public string EasyMartCode { get; set; }

        /// <summary>
        /// Tên đầy đủ của siêu thị / chi nhánh
        /// </summary>
        public string EasyMartName { get; set; }

        /// <summary>
        /// Email liên hệ chính thức
        /// </summary>
        public string? ContactEmail { get; set; }

        /// <summary>
        /// Số điện thoại liên hệ
        /// </summary>
        public string? ContactPhone { get; set; }

        /// <summary>
        /// Trạng thái hoạt động: 1 - Đang hoạt động, 0 - Tạm dừng kinh doanh
        /// </summary>
        public int IsActive { get; set; } = 1;

        /// <summary>
        /// Ngày hết hạn hợp đồng phần mềm / license của siêu thị
        /// </summary>
        public DateTime? ExpiredDate { get; set; }

        /// <summary>
        /// Thời điểm khởi tạo bản ghi siêu thị
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Thời điểm cập nhật thông tin gần nhất
        /// </summary>
        public DateTime? ModifiedDate { get; set; }

        /// <summary>
        /// Trạng thái xóa mềm: 1 - Đã xóa, 0 - Chưa xóa
        /// </summary>
        public int IsDeleted { get; set; } = 0;
    }
}