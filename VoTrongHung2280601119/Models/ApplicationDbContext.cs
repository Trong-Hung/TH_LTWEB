using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // Cần cho IdentityDbContext
using Microsoft.EntityFrameworkCore;

namespace VoTrongHung2280601119.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Warehouse> Warehouses { get; set; }
        public DbSet<OrderDistribution> OrderDistributions { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<Message> Messages { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Cấu hình các mối quan hệ phức tạp hơn nếu cần (ví dụ: khóa composite)
            // Ví dụ: Thiết lập ProductId và OrderDistributionId là khóa chính trong OrderItem (nếu không có Id)
            // builder.Entity<OrderItem>().HasKey(oi => new { oi.OrderDistributionId, oi.ProductId });
            // Cấu hình mối quan hệ nhiều-nhiều giữa User và Room
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.ChatRooms)
                .WithMany(r => r.Users);
        }
    }
}