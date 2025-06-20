using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Cần cho [ForeignKey]

namespace VoTrongHung2280601119.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderDistributionId { get; set; } // Foreign Key đến OrderDistribution
        [ForeignKey("OrderDistributionId")]
        public OrderDistribution? OrderDistribution { get; set; } // Navigation Property

        public int ProductId { get; set; } // Foreign Key đến Product
        [ForeignKey("ProductId")]
        public Product? Product { get; set; } // Navigation Property

        public int Quantity { get; set; } // Số lượng sản phẩm
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } // Giá tại thời điểm đặt hàng (để tránh thay đổi giá sản phẩm ảnh hưởng đến đơn hàng cũ)
        public int WarehouseId { get; set; } // Kho mà sản phẩm được lấy từ đó
        [ForeignKey("WarehouseId")]
        public Warehouse? Warehouse { get; set; } // Navigation Property
    }
}