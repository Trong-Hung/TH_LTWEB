using System.Collections.Generic;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);

        Task<Category> GetBySlugAsync(string slug); // <-- Thêm dòng này để lấy danh mục theo slug

        Task<Category> GetByNameAsync(string name);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }
}