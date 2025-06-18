using System.Collections.Generic;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}