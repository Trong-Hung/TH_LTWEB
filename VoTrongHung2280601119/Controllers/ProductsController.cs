using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần cho SelectList
using System.Collections.Generic;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;

namespace VoTrongHung2280601119.Controllers // Namespace này là gốc
{
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseRepository _warehouseRepository; // Cần để chọn kho khi đặt hàng

        public ProductsController(IProductRepository productRepository, IWarehouseRepository warehouseRepository)
        {
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
        }

        // GET: /Product hoặc / (Customer View - Trang chủ, danh sách sản phẩm)
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // GET: /Product/Details/5 (Customer View - Chi tiết sản phẩm)
        public async Task<IActionResult> Details(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Lấy danh sách kho để người dùng chọn khi đặt hàng
            var warehouses = await _warehouseRepository.GetAllAsync();
            ViewBag.Warehouses = new SelectList(warehouses, "Id", "Name");

            return View(product);
        }

      
    }
}