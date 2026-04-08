using BASE.Service.Core.Attribute;
using BASE.Service.Core.Model;
using System.ComponentModel.DataAnnotations;

namespace EasyMart.Model.Dictionary
{
    [ConfigTable("di_unit", "")]
    public class Unit : BaseModel
    {
        /// <summary>
        /// Khóa chính của bảng
        /// </summary>
        [Key]
        public Guid UnitID { get; set; }

        /// <summary>
        /// Tên đơn vị tính (VD: Cái, Hộp, Thùng, Kg...)
        /// </summary>
        public string UnitName { get; set; }

        /// <summary>
        /// Mô tả / Diễn giải đơn vị tính
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Trạng thái: 1 - Đang sử dụng, 2 - Ngừng sử dụng
        /// </summary>
        public int Status { get; set; } = 1;
    }
}
