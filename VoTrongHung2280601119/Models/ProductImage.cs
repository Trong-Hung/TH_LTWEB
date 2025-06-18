using System.ComponentModel.DataAnnotations;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        [Required, StringLength(500)]
        public string Url { get; set; } // Đường dẫn đến ảnh
        public int ProductId { get; set; } // Foreign Key đến Product
        public Product? Product { get; set; } // Navigation Property
    }
}