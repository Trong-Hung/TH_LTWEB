using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VoTrongHung2280601119.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        // Thêm các thuộc tính khác của người dùng nếu cần
    }
}