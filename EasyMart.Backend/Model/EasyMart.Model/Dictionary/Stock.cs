using BASE.Service.Core.Attribute;
using BASE.Service.Core.Enum;
using BASE.Service.Core.Model;
using System.ComponentModel.DataAnnotations;

namespace EasyMart.Model.Dictionary
{
    [ConfigTable("di_stock", "")]
    public class Stock : BaseModel
    {
        [Key]
        public Guid StockID { get; set; }

        public string StockCode { get; set; }

        public string StockName { get; set; }

        public string Address { get; set; }

        public string Description { get; set; }

        /// <summary>
        /// 1: Đang sử dụng, 2: Ngừng sử dụng
        /// </summary>
        public RecordStatus Status { get; set; } = RecordStatus.Active;
    }
}
