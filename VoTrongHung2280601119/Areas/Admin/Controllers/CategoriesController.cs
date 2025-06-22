using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;
using VoTrongHung2280601119.Helpers;

namespace VoTrongHung_2280601119.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoriesController : Controller
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        // GET: Admin/Category
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return View(categories);
        }

        // GET: Admin/Category/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            // <<< BẮT ĐẦU KHỐI CẢI TIẾN >>>
            // Kiểm tra xem Name đã tồn tại chưa
            var existingCategory = await _categoryRepository.GetByNameAsync(category.Name);
            if (existingCategory != null)
            {
                // Nếu đã tồn tại, thêm lỗi vào ModelState và trả về View
                ModelState.AddModelError("Name", "Tên danh mục này đã tồn tại.");
            }
            // <<< KẾT THÚC KHỐI CẢI TIẾN >>>

            if (ModelState.IsValid)
            {

                category.Slug = UrlHelper.GenerateSlug(category.Name);
                await _categoryRepository.AddAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Action cập nhật Slug cho các danh mục cũ
        public async Task<IActionResult> UpdateAllCategorySlugs()
        {
            var categories = await _categoryRepository.GetAllAsync();
            foreach (var category in categories)
            {
                if (string.IsNullOrEmpty(category.Slug))
                {
                    category.Slug = UrlHelper.GenerateSlug(category.Name);
                    await _categoryRepository.UpdateAsync(category);
                }
            }
            return Content("Đã cập nhật Slug cho tất cả danh mục.");
        }

        // GET: Admin/Categories/Edit/ten-danh-muc
        [Route("Admin/Categories/Edit/{slug}")]
        public async Task<IActionResult> Edit(string slug)
        {
            var category = await _categoryRepository.GetBySlugAsync(slug);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/Categories/Edit/ten-danh-muc
        // <<< BẮT ĐẦU KHỐI SỬA ĐỔI CHO ACTION EDIT (POST) >>>
        [HttpPost("Admin/Categories/Edit/{slug}")] // <<< THAY ĐỔI 1: Thêm route tường minh với slug
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string slug, Category category) // <<< THAY ĐỔI 2: Đổi tham số từ id sang slug
        {
            // <<< THAY ĐỔI 3: Lấy đối tượng gốc từ DB bằng Id từ form
            var existingCategory = await _categoryRepository.GetByIdAsync(category.Id);
            if (existingCategory == null)
            {
                return NotFound();
            }

            // Kiểm tra slug từ URL có khớp với đối tượng gốc không
            if (slug != existingCategory.Slug)
            {
                return BadRequest();
            }

            // Xóa lỗi của Slug khỏi ModelState vì chúng ta tự tạo nó
            ModelState.Remove("Slug");

            if (ModelState.IsValid)
            {
                try
                {
                    // <<< THAY ĐỔI 4: Cập nhật trên đối tượng đã tồn tại
                    existingCategory.Name = category.Name;
                    existingCategory.Slug = UrlHelper.GenerateSlug(category.Name); // Tạo slug mới nếu tên thay đổi

                    await _categoryRepository.UpdateAsync(existingCategory);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _categoryRepository.GetByIdAsync(category.Id) == null)
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
            return View(category);
        }
        // <<< KẾT THÚC KHỐI SỬA ĐỔI CHO ACTION EDIT (POST) >>>


        // GET: Admin/Categories/Delete/ten-danh-muc
        [Route("Admin/Categories/Delete/{slug}")]
        public async Task<IActionResult> Delete(string slug)
        {
            var category = await _categoryRepository.GetBySlugAsync(slug);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/Categories/Delete/ten-danh-muc
        // <<< BẮT ĐẦU KHỐI SỬA ĐỔI CHO ACTION DELETECONFIRMED (POST) >>>
        [HttpPost("Admin/Categories/Delete/{slug}"), ActionName("Delete")] // <<< THAY ĐỔI 1: Thêm route tường minh với slug
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string slug) // <<< THAY ĐỔI 2: Đổi tham số từ id sang slug
        {
            // <<< THAY ĐỔI 3: Tìm danh mục bằng slug để xóa
            var category = await _categoryRepository.GetBySlugAsync(slug);
            if (category != null)
            {
                // <<< THAY ĐỔI 4: Vẫn dùng Id của đối tượng tìm được để xóa
                await _categoryRepository.DeleteAsync(category.Id);
            }
            return RedirectToAction(nameof(Index));
        }
        // <<< KẾT THÚC KHỐI SỬA ĐỔI CHO ACTION DELETECONFIRMED (POST) >>>
    }
}