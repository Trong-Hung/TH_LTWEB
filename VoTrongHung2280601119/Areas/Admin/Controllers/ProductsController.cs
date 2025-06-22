using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;

namespace VoTrongHung_2280601119.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ApplicationDbContext _context;

        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IWebHostEnvironment webHostEnvironment, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageUrlFile)
        {
            if (ModelState.IsValid)
            {
                if (imageUrlFile != null)
                {
                    product.ImageUrl = await SaveImage(imageUrlFile);
                }
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageUrlFile)
        {
            if (id != product.Id) return NotFound();
            if (imageUrlFile == null) ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageUrlFile != null)
                    {
                        if (!string.IsNullOrEmpty(product.ImageUrl)) DeleteImage(product.ImageUrl);
                        product.ImageUrl = await SaveImage(imageUrlFile);
                    }
                    else
                    {
                        var existingProduct = await _productRepository.GetByIdAsync(id);
                        if (existingProduct != null) product.ImageUrl = existingProduct.ImageUrl;
                    }
                    await _productRepository.UpdateAsync(product);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _productRepository.GetByIdAsync(product.Id) == null) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null) return NotFound();
            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.ImageUrl)) DeleteImage(product.ImageUrl);
                await _productRepository.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return "/images/" + fileName;
        }

        private void DeleteImage(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }
        }

        public IActionResult ExportProductsToExcel()
        {
            var products = _context.Products.Include(p => p.Category).ToList();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Products");

            worksheet.Cell(1, 1).Value = "Mã SP";
            worksheet.Cell(1, 2).Value = "Tên sản phẩm";
            worksheet.Cell(1, 3).Value = "Mô tả";
            worksheet.Cell(1, 4).Value = "Giá";
            worksheet.Cell(1, 5).Value = "Tồn kho";
            worksheet.Cell(1, 6).Value = "Đơn vị";
            worksheet.Cell(1, 7).Value = "Danh mục";

            for (int i = 0; i < products.Count; i++)
            {
                var row = i + 2;
                var product = products[i];
                worksheet.Cell(row, 1).Value = product.Code;
                worksheet.Cell(row, 2).Value = product.Name;
                worksheet.Cell(row, 3).Value = product.Description;
                worksheet.Cell(row, 4).Value = product.Price;
                worksheet.Cell(row, 5).Value = product.Stock;
                worksheet.Cell(row, 6).Value = product.Unit;
                worksheet.Cell(row, 7).Value = product.Category?.Name ?? "Chưa phân loại";
            }

            worksheet.Columns().AdjustToContents();
            worksheet.RangeUsed().SetAutoFilter();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "DanhSachSanPham.xlsx");
        }

        [HttpPost]
        public IActionResult ImportProductsFromExcel(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using var stream = new MemoryStream();
                file.CopyTo(stream);
                using var workbook = new XLWorkbook(stream);
                var worksheet = workbook.Worksheet(1);
                var rows = worksheet.RowsUsed().Skip(1); // bỏ dòng tiêu đề

                foreach (var row in rows)
                {
                    var code = row.Cell(1).GetString();          // Mã SP
                    var name = row.Cell(2).GetString();          // Tên sản phẩm
                    var description = row.Cell(3).GetString();   // Mô tả
                    var price = row.Cell(4).GetValue<decimal>(); // Giá
                    var stock = row.Cell(5).GetValue<int>();     // Tồn kho
                    var unit = row.Cell(6).GetString();          // Đơn vị
                    var categoryName = row.Cell(7).GetString();  // Danh mục

                    // Tìm category theo tên, nếu không có thì gán null (bạn có thể xử lý tạo mới category nếu muốn)
                    var category = _context.Categories.FirstOrDefault(c => c.Name == categoryName);

                    var product = new Product
                    {
                        Code = code,
                        Name = name,
                        Description = description,
                        Price = price,
                        Stock = stock,
                        Unit = unit,
                        CategoryId = category?.Id ?? 0, // nếu không có category thì 0 (hoặc bạn có thể xử lý khác)
                    };

                    _context.Products.Add(product);
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
