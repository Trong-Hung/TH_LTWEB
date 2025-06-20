using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VoTrongHung2280601119.Models
{
    public class OrderInfoModel // Đây là model để truyền thông tin Order cho Momo
    {
        public string OrderId { get; set; } // Mã đơn hàng (sẽ được tạo tự động)
        public long Amount { get; set; } // Tổng tiền (long vì Momo dùng tiền nguyên)
        public string OrderInfo { get; set; } // Thông tin đơn hàng gửi Momo
        public string FullName { get; set; } // Tên khách hàng đặt hàng

        // Các thông tin khác nếu cần cho riêng bạn
        // public string UserId { get; set; }
        // public string ShippingAddress { get; set; }
    }
}