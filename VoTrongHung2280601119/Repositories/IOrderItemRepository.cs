using System.Collections.Generic;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.Repositories
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetAllAsync();
        Task<OrderItem> GetByIdAsync(int id);
        Task AddAsync(OrderItem orderItem);
        Task UpdateAsync(OrderItem orderItem);
        Task DeleteAsync(int id);
    }
}