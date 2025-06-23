using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VoTrongHung2280601119.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string? FullName { get; set; }
        public ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
        // Thêm các thuộc tính khác của người dùng nếu cần
    }
}