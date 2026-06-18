using BASE.Service.Core.Attribute;
using System.ComponentModel.DataAnnotations;

namespace BASE.Service.Core.Model
{
    /// <summary>
    /// Model thực thể đại diện cho việc gán quyền/liên kết giữa Người dùng và Siêu thị (Bảng trung gian Many-to-Many).
    /// Ánh xạ trực tiếp với bảng "user_easymart_assignment" trong cơ sở dữ liệu.
    /// </summary>
    [ConfigTable("user_easymart_assignment", "")]
    public class UserEasyMartAssignment : BaseModel
    {
        /// <summary>
        /// ID người dùng (Khóa chính phức hợp / Khóa ngoại liên kết tới bảng user)
        /// </summary>
        [Key]
        public Guid UserID { get; set; }

        /// <summary>
        /// ID Siêu thị / Đơn vị thành viên (Khóa chính phức hợp / Liên kết logic tới bảng easymart)
        /// </summary>
        [Key]
        public Guid EasyMartID { get; set; }

        /// <summary>
        /// Thời điểm gán người dùng vào siêu thị này
        /// </summary>
        public DateTime AssignedAt { get; set; } = DateTime.Now;
    }
}