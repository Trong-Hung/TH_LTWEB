using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần cho SelectList
using System.Collections.Generic;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;

namespace VoTrongHung_2280601119.Controllers // Namespace này là gốc
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseRepository _warehouseRepository;

        public ProductController(IProductRepository productRepository, IWarehouseRepository warehouseRepository)
        {
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
        }

        // Public Product Controller chỉ dùng cho Details
        // Action Index() sẽ nằm trong HomeController để hiển thị trang chủ

        // GET: /san-pham/ten-san-pham-slug
        [Route("san-pham/{slug}")] // <-- THAY ĐỔI 1: THÊM ROUTE MỚI
        public async Task<IActionResult> Details(string slug) // <-- THAY ĐỔI 2: ĐỔI THAM SỐ TỪ (int id) SANG (string slug)
        {
            // THAY ĐỔI 3: TÌM SẢN PHẨM BẰNG SLUG THAY VÌ ID
            var product = await _productRepository.GetBySlugAsync(slug);

            if (product == null)
            {
                return NotFound();
            }

            // Giữ nguyên phần lấy danh sách kho
            var warehouses = await _warehouseRepository.GetAllAsync();
            ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name");

            return View(product);
        }

        [HttpGet("api/search-suggestions")] // Định nghĩa đường dẫn cho API
        public async Task<IActionResult> SearchSuggestions(string term)
        {
            if (string.IsNullOrEmpty(term) || term.Length < 2)
            {
                return Ok(new List<object>()); // Trả về danh sách rỗng nếu từ khóa quá ngắn
            }

            // Dùng lại phương thức SearchAsync mạnh mẽ mà chúng ta đã tạo ở các bước trước
            var products = await _productRepository.SearchAsync(term);

            // Chỉ chọn những thông tin cần thiết để trả về (tên, slug, ảnh)
            // và giới hạn 5 kết quả để danh sách gợi ý không quá dài
            var suggestions = products.Select(p => new {
                name = p.Name,
                slug = p.Slug,
                imageUrl = p.ImageUrl
            }).Take(5);

            return Ok(suggestions); // Trả về dữ liệu dưới dạng JSON
        }
    }
}