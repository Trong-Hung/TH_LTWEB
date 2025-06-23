using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;


namespace VoTrongHung_2280601119.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;
        private readonly IProductRepository _productRepository; // Bổ sung: Inject IProductRepository

        public HomeController(ILogger<HomeController> logger, IProductRepository productRepository)
        {
            _logger = logger;
            _productRepository = productRepository; // Gán IProductRepository
        }

        // Sửa lại Action Index để nhận tham số tìm kiếm
        public async Task<IActionResult> Index(string searchString)
        {
            IEnumerable<Product> products;

            if (!string.IsNullOrEmpty(searchString))
            {
                // Nếu có từ khóa, gọi phương thức tìm kiếm mới trong repository
                products = await _productRepository.SearchAsync(searchString);
            }
            else
            {
                // Nếu không, lấy tất cả sản phẩm như cũ
                products = await _productRepository.GetAllAsync();
            }

            ViewData["CurrentFilter"] = searchString; // Lưu lại từ khóa để hiển thị trên View

            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}