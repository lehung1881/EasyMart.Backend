using BASE.Service.Core.Enum;

namespace BASE.Service.Core.Model
{
    /// <summary>
    /// Chuẩn hóa response trả về cho tất cả các API trong hệ thống.
    /// Bao gồm trạng thái thành công/thất bại, mã lỗi, thông báo và dữ liệu.
    /// </summary>
    public class ServiceResponse
    {
        #region Property

        /// <summary>
        /// Trạng thái xử lý: <c>true</c> nếu thành công, <c>false</c> nếu thất bại.
        /// </summary>
        public bool Success { get; set; } = true;

        /// <summary>
        /// Mã phản hồi tương ứng với <see cref="ServiceResponseCode"/>.
        /// Mặc định là <see cref="ServiceResponseCode.Success"/>.
        /// </summary>
        public ServiceResponseCode ResponseCode { get; set; } = 0;

        /// <summary>
        /// Thông báo hiển thị cho người dùng cuối.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Thông báo hệ thống dành cho developer hoặc log.
        /// Không nên hiển thị trực tiếp cho người dùng.
        /// </summary>
        public string SystemMessage { get; set; }

        /// <summary>
        /// Chi tiết lỗi bổ sung (nếu có).
        /// Có thể chứa danh sách lỗi validation hoặc exception message.
        /// </summary>
        public object ErrorMessage { get; set; }

        /// <summary>
        /// Dữ liệu trả về khi xử lý thành công.
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// Danh sách kết quả validate (nếu có lỗi dữ liệu đầu vào).
        /// </summary>
        public List<ValidateResult> ValidateInfo { get; set; }

        #endregion

        #region Method

        /// <summary>
        /// Đánh dấu response thành công và gán dữ liệu trả về.
        /// </summary>
        /// <param name="data">Dữ liệu trả về cho client. Mặc định là <c>null</c>.</param>
        public void OnSuccess(object data = null)
        {
            this.Success = true;
            this.Data = data;
        }

        /// <summary>
        /// Đánh dấu response thất bại với thông báo lỗi đơn giản.
        /// </summary>
        /// <param name="msg">Thông báo lỗi hiển thị cho người dùng.</param>
        public void OnError(string msg = "")
        {
            this.Success = false;
            this.Message = msg;
        }

        /// <summary>
        /// Đánh dấu response thất bại với mã lỗi, thông báo người dùng và thông báo hệ thống.
        /// </summary>
        /// <param name="responseCode">Mã lỗi theo <see cref="ServiceResponseCode"/>.</param>
        /// <param name="msg">Thông báo lỗi hiển thị cho người dùng.</param>
        /// <param name="systemMsg">Thông báo hệ thống dành cho developer hoặc log.</param>
        public void OnError(ServiceResponseCode responseCode, string msg = "", string systemMsg = "")
        {
            this.Success = false;
            this.ResponseCode = responseCode;
            this.Message = msg;
            this.SystemMessage = systemMsg;
        }

        #endregion
    }
}
