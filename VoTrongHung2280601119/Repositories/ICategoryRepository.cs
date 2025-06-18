using System.Collections.Generic;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category> GetByIdAsync(int id);
        Task AddAsync(Category category);
        Task UpdateAsync(Category category);
        Task DeleteAsync(int id);
    }
}