using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace VoTrongHung2280601119.Models
{
    public class Category
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Tên danh mục là bắt buộc."), StringLength(50)]
        public string Name { get; set; }

        public string Slug { get; set; } // <-- Thêm dòng này để lưu slug URL thân thiện
        public List<Product>? Products { get; set; } // Quan hệ 1-n với Product
    }
}