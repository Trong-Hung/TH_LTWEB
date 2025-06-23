using System.Collections.Generic;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();
        Task<Product> GetByIdAsync(int id);

        // THÊM DÒNG NÀY VÀO để lấy sản phẩm theo slug
        Task<Product> GetBySlugAsync(string slug);

        Task<IEnumerable<Product>> SearchAsync(string keyword); // <-- Thêm dòng này tìm kiếm sản phẩm

        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
    }
}