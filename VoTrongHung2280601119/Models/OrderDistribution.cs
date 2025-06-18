using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Cần cho [ForeignKey]
using VoTrongHung2280601119.Models; // Đảm bảo namespace này khớp với project của bạn

namespace VoTrongHung2280601119.Models
{
    public class OrderDistribution
    {
        public int Id { get; set; }
        // [Required] // ĐẢM BẢO DÒNG NÀY ĐÃ BỊ XÓA HOẶC COMMENT
        public string? CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public ApplicationUser? Customer { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc."), StringLength(200)]
        public string ShippingAddress { get; set; }
        [StringLength(500)]
        public string? Notes { get; set; }

        [Required, StringLength(50)]
        public string Status { get; set; } = "Chờ Xác nhận";

        [Required(ErrorMessage = "Tổng tiền là bắt buộc."), Column(TypeName = "decimal(18, 2)")]
        public decimal TotalPrice { get; set; }

        public List<OrderItem>? OrderItems { get; set; }
    }
}