using Microsoft.EntityFrameworkCore;
using T3_VoTrongHung_2280601119.Models;

namespace T3_VoTrongHung_2280601119.Repositories
{
    public class EFCategoryRepository : ICategoryRepository
    {

        private readonly ApplicationDbContext _context;
        public EFCategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            // Lấy tất cả các category từ database
            return await _context.Categorie.ToListAsync();
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            // Lấy category theo id
            return await _context.Categorie.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Category category)
        {
            // Thêm mới category
            await _context.Categorie.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            // Cập nhật category
            _context.Categorie.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            // Xóa category theo id
            var category = await _context.Categorie.FindAsync(id);
            if (category != null)
            {
                _context.Categorie.Remove(category);
                await _context.SaveChangesAsync();
            }
        }
    }
}
