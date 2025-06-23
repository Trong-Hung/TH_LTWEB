using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;

namespace VoTrongHung2280601119.Repositories
{
    public class EFProductRepository : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public EFProductRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }

        public async Task<Product> GetByIdAsync(int id)
        {
            return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
        }

        // THÊM PHƯƠNG THỨC MỚI NÀY VÀO để lấy sản phẩm theo slug
        public async Task<Product> GetBySlugAsync(string slug)
        {
            // Tìm sản phẩm có slug khớp, và lấy luôn dữ liệu Category nếu có
            return await _context.Products
                                 .Include(p => p.Category)
                                 .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        // Thêm phương thức mới này vào lớp EFProductRepository tìm kiếm
        // Thay thế phương thức SearchAsync cũ bằng phương thức này
        // Thêm phương thức mới này vào lớp EFProductRepository

        public async Task<IEnumerable<Product>> SearchAsync(string keyword)
        {
            var lowerKeyword = keyword.ToLower(); // Chuyển từ khóa về chữ thường một lần

            return await _context.Products
                                 .Include(p => p.Category) // Lấy kèm dữ liệu của Category để tìm kiếm
                                 .Where(p =>
                                     // Tìm trong Tên sản phẩm
                                     p.Name.ToLower().Contains(lowerKeyword) ||

                                     // Tìm trong Mô tả sản phẩm
                                     p.Description.ToLower().Contains(lowerKeyword) ||

                                     // Tìm trong Đơn vị tính của sản phẩm
                                     (p.Unit != null && p.Unit.ToLower().Contains(lowerKeyword)) ||

                                     // Tìm trong Tên Danh mục của sản phẩm
                                     (p.Category != null && p.Category.Name.ToLower().Contains(lowerKeyword))
                                 )
                                 .ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            _context.Products.Add(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
        }


    }
}