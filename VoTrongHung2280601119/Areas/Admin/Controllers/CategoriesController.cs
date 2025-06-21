using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
            if (ModelState.IsValid)
            {
                category.Slug = UrlHelper.GenerateSlug(category.Name); // <-- Thêm dòng này
                await _categoryRepository.AddAsync(category);
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // Thêm Action này vào CategoriesController.cs
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
        [Route("Admin/Categories/Edit/{slug}")] // Thêm Route
        public async Task<IActionResult> Edit(string slug) // Đổi tham số
        {
            var category = await _categoryRepository.GetBySlugAsync(slug); // Tìm bằng slug
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id) { return NotFound(); }

            if (ModelState.IsValid)
            {
                try
                {
                    category.Slug = UrlHelper.GenerateSlug(category.Name); // <-- Thêm dòng này
                    await _categoryRepository.UpdateAsync(category);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _categoryRepository.GetByIdAsync(category.Id) == null) { return NotFound(); }
                    else { throw; }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Admin/Categories/Delete/ten-danh-muc
        [Route("Admin/Categories/Delete/{slug}")] // Thêm Route
        public async Task<IActionResult> Delete(string slug) // Đổi tham số
        {
            var category = await _categoryRepository.GetBySlugAsync(slug); // Tìm bằng slug
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/Category/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                await _categoryRepository.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }
    }
}