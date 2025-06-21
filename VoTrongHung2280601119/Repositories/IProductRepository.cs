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

        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(int id);
        // THÊM ĐỊNH NGHĨA PHƯƠNG THỨC MỚI VÀO ĐÂY
        Task<IEnumerable<Product>> GetByCategoryIdAsync(int categoryId);

        // THÊM DÒNG NÀY VÀO
        Task<IEnumerable<Product>> SearchAsync(string keyword); // <-- Thêm dòng này tìm kiếm sản phẩm
    }
}