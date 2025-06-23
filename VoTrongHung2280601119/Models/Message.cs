using System.ComponentModel.DataAnnotations;

namespace VoTrongHung2280601119.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        // THÊM THUỘC TÍNH NÀY: Lưu tên của người gửi
        public string UserName { get; set; } = string.Empty;

        // Thuộc tính này có thể là null
        public string? UserId { get; set; }

        // THÊM THUỘC TÍNH NÀY: Lưu tên phòng chat 
        public string RoomName { get; set; } = string.Empty;
    }
}
