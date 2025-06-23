namespace VoTrongHung2280601119.Models
{
    public class ChatRoom
    {
        public int Id { get; set; }
        public string Name { get; set; } // Tên phòng chat, có thể là tên nhóm hoặc null cho chat 1-1
        public ICollection<Message> Messages { get; set; }
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
