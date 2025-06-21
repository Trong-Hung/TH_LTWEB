using Microsoft.AspNetCore.Authorization; // Cần cho [Authorize]
using Microsoft.AspNetCore.Hosting; // Cần cho IWebHostEnvironment
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Cần cho DbUpdateConcurrencyException
using System.IO; // Cần cho xử lý file ảnh
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;
using VoTrongHung2280601119.Helpers; // Thêm using này ở đầu file

namespace VoTrongHung_2280601119.Areas.Admin.Controllers
{
    [Area("Admin")] // Đánh dấu đây là Controller cho Admin Area
    [Authorize(Roles = SD.Role_Admin)] // CHỈ CÓ ROLE_ADMIN MỚI ĐƯỢC TRUY CẬP CONTROLLER NÀY
    public class ProductsController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment; // Để truy cập đường dẫn wwwroot

        public ProductsController(IProductRepository productRepository, ICategoryRepository categoryRepository, IWebHostEnvironment webHostEnvironment)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Product
        // Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // GET: Admin/Product/Create
        // Hiển thị form tạo sản phẩm mới
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }



        // POST: Admin/Product/Create
        // Xử lý tạo sản phẩm mới và upload ảnh
        [HttpPost]
        [ValidateAntiForgeryToken] // Chống tấn công CSRF
        public async Task<IActionResult> Create(Product product, IFormFile? imageUrlFile) // IFormFile? để nhận file upload
        {
            if (ModelState.IsValid)
            {
                // Tự động tạo slug từ Name
                product.Slug = UrlHelper.GenerateSlug(product.Name);

                if (imageUrlFile != null)
                {
                    product.ImageUrl = await SaveImage(imageUrlFile); // Gọi hàm lưu ảnh
                }
                await _productRepository.AddAsync(product);
                return RedirectToAction(nameof(Index));
            }
            // Nếu có lỗi validation, load lại danh mục và hiển thị lại form
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        //Các sản phẩm đã có sẵn trong database (tạo từ trước)
        //sẽ chưa có Slug → cần viết đoạn code để
        //cập nhật Slug cho những sản phẩm
        public async Task<IActionResult> UpdateAllSlugs()
        {
            var products = await _productRepository.GetAllAsync();
            foreach (var p in products)
            {
                if (string.IsNullOrEmpty(p.Slug))
                {
                    p.Slug = UrlHelper.GenerateSlug(p.Name);
                    await _productRepository.UpdateAsync(p);
                }
            }
            return Content(" Đã cập nhật Slug cho tất cả sản phẩm chưa có Slug.");

        }

        // GET: Admin/Products/Edit/ten-san-pham
        [Route("Admin/Products/Edit/{slug}")] // Thêm Route mới
        public async Task<IActionResult> Edit(string slug) // Đổi tham số thành string slug
        {
            // Tìm sản phẩm bằng slug
            var product = await _productRepository.GetBySlugAsync(slug);
            if (product == null)
            {
                return NotFound();
            }
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/Product/Edit/5
        // Xử lý cập nhật sản phẩm và ảnh
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile? imageUrlFile)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            // Nếu không có file ảnh mới được upload, không validate trường ImageUrl
            if (imageUrlFile == null)
            {
                ModelState.Remove("ImageUrl");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Tự động tạo slug từ Name khi sửa
                    product.Slug = UrlHelper.GenerateSlug(product.Name);

                    if (imageUrlFile != null) // Nếu có ảnh mới upload
                    {
                        // Xóa ảnh cũ nếu tồn tại
                        if (!string.IsNullOrEmpty(product.ImageUrl))
                        {
                            DeleteImage(product.ImageUrl);
                        }
                        // Lưu ảnh mới
                        product.ImageUrl = await SaveImage(imageUrlFile);
                    }
                    else // Không có ảnh mới, giữ nguyên đường dẫn ảnh cũ từ DB
                    {
                        var existingProduct = await _productRepository.GetByIdAsync(id);
                        if (existingProduct != null)
                        {
                            product.ImageUrl = existingProduct.ImageUrl;
                        }
                    }
                    await _productRepository.UpdateAsync(product);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _productRepository.GetByIdAsync(product.Id) == null)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // Nếu có lỗi validation, load lại danh mục và hiển thị lại form
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Admin/Products/Delete/ten-san-pham
        [Route("Admin/Products/Delete/{slug}")] // Thêm Route mới
        public async Task<IActionResult> Delete(string slug) // Đổi tham số thành string slug
        {
            // Tìm sản phẩm bằng slug
            var product = await _productRepository.GetBySlugAsync(slug);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Admin/Product/Delete/5
        // Xử lý xóa sản phẩm và ảnh vật lý
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product != null)
            {
                // Xóa ảnh cũ nếu tồn tại
                if (!string.IsNullOrEmpty(product.ImageUrl))
                {
                    DeleteImage(product.ImageUrl);
                }
                await _productRepository.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        // Helper method để lưu hình ảnh vào wwwroot/images
        private async Task<string> SaveImage(IFormFile image)
        {
            // Tạo thư mục wwwroot/images nếu chưa tồn tại
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Tạo tên file duy nhất bằng GUID
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            // Lưu file vào thư mục
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            // Trả về đường dẫn tương đối để lưu vào database
            return "/images/" + fileName;
        }

        // Helper method để xóa hình ảnh vật lý từ wwwroot/images
        private void DeleteImage(string imageUrl)
        {
            if (!string.IsNullOrEmpty(imageUrl))
            {
                // Tạo đường dẫn vật lý đầy đủ từ đường dẫn tương đối
                string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl.TrimStart('/'));
                // Kiểm tra và xóa file
                if (System.IO.File.Exists(oldFilePath))
                {
                    System.IO.File.Delete(oldFilePath);
                }
            }
        }
    }
}