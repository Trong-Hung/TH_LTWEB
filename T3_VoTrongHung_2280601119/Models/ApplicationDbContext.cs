using Microsoft.EntityFrameworkCore;
namespace T3_VoTrongHung_2280601119.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        {
        }

        public DbSet<Product> Product { get; set; }
        public DbSet<Category> Categorie { get; set; }
        public DbSet<ProductImage> ProductImage { get; set; }
    }
}
