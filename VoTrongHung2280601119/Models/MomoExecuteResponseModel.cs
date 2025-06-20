namespace VoTrongHung2280601119.Models
{
    public class MomoExecuteResponseModel
    {
        public string OrderId { get; set; }
        public string Amount { get; set; }
        public string FullName { get; set; } // Thường là tên khách hàng
        public string OrderInfo { get; set; } // Thông tin đặt hàng
        public string Message { get; set; } // Thông báo từ Momo
    }
}