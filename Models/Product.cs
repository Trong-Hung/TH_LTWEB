using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Cần cho [Column]

namespace VoTrongHung2280601119.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Mã sản phẩm là bắt buộc."), StringLength(50)]
        public string Code { get; set; } // Mã sản phẩm
        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc."), StringLength(100)]
        public string Name { get; set; }
        [Required(ErrorMessage = "Mô tả là bắt buộc."), StringLength(500)]
        public string Description { get; set; }
        [Required(ErrorMessage = "Giá bán là bắt buộc."), Range(0.01, 100000000.00, ErrorMessage = "Giá phải lớn hơn 0.")]
        [Column(TypeName = "decimal(18, 2)")] // Định dạng giá trị tiền tệ trong SQL Server
        public decimal Price { get; set; }
        [Required(ErrorMessage = "Tồn kho là bắt buộc."), Range(0, int.MaxValue, ErrorMessage = "Tồn kho không hợp lệ.")]
        public int Stock { get; set; } // Tồn kho
        [Required(ErrorMessage = "Đơn vị là bắt buộc."), StringLength(20)]
        public string Unit { get; set; } // Đơn vị (VD: cái, bộ)
        public string? ImageUrl { get; set; } // Đường dẫn ảnh đại diện

        public int CategoryId { get; set; } // Foreign Key đến Category
        public Category? Category { get; set; } // Navigation Property

        public List<ProductImage>? Images { get; set; } // Quan hệ 1-n với ProductImage (Ảnh chi tiết)
        // Nếu có Nhà cung cấp hoặc Kho mặc định, thêm FK ở đây
        public int? SupplierId { get; set; } // Có thể thêm Supplier Model sau
        public int? DefaultWarehouseId { get; set; } // Có thể thêm Warehouse Model sau
    }
}