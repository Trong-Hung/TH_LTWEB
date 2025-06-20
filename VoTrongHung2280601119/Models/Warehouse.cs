using System.ComponentModel.DataAnnotations;

namespace VoTrongHung2280601119.Models
{
    public class Warehouse
    {
        public int Id { get; set; } // kho_id
        [Required(ErrorMessage = "Tên kho là bắt buộc."), StringLength(100)]
        public string Name { get; set; } // Tên kho
        [Required(ErrorMessage = "Địa chỉ kho là bắt buộc."), StringLength(200)]
        public string Address { get; set; } // Địa chỉ
        [Required(ErrorMessage = "Sức chứa là bắt buộc."), Range(0, int.MaxValue, ErrorMessage = "Sức chứa không hợp lệ.")]
        public int Capacity { get; set; } // Sức chứa
    }
}