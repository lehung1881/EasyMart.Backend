using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASE.Service.Core.Model
{
    /// <summary>
    /// Model đại diện cho kết quả trả về của phân trang.
    /// </summary>
    public class PagingResponse
    {
        public PagingResponse(object pageData, int total)
        {
            PageData = pageData;
            Total = total;
        }

        /// <summary>
        /// Dữ liệu của trang hiện tại.
        /// </summary>
        public object PageData { get; set; }

        /// <summary>
        /// Tổng số bản ghi thỏa mãn điều kiện.
        /// </summary>
        public int Total { get; set; }
    }
}
