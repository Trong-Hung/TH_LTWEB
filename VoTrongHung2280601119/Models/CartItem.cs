using System.ComponentModel.DataAnnotations; // Có thể cần nếu bạn muốn thêm validation cho CartItem

namespace VoTrongHung2280601119.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } // Tên sản phẩm
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; } // Ảnh sản phẩm
        public int WarehouseId { get; set; } // Kho đã chọn
        public string? WarehouseName { get; set; } // Tên kho (để hiển thị)

        public decimal Subtotal => Quantity * Price; // Tổng phụ cho mặt hàng này
    }
}