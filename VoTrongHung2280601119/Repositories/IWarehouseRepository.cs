using System.Collections.Generic;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.Repositories
{
    public interface IWarehouseRepository
    {
        Task<IEnumerable<Warehouse>> GetAllAsync();
        Task<Warehouse> GetByIdAsync(int id);

        Task<Warehouse> GetBySlugAsync(string slug); // <-- Thêm dòng này
        Task AddAsync(Warehouse warehouse);
        Task UpdateAsync(Warehouse warehouse);
        Task DeleteAsync(int id);
    }
}