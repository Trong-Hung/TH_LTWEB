using System.Collections.Generic;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.Repositories
{
    public interface IOrderDistributionRepository
    {
        Task<IEnumerable<OrderDistribution>> GetAllAsync();
        Task<OrderDistribution> GetByIdAsync(int id);
        Task AddAsync(OrderDistribution order);
        Task UpdateAsync(OrderDistribution order);
        Task DeleteAsync(int id);
    }
}