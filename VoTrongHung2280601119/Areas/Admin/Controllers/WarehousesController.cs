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
    public class WarehousesController : Controller
    {
        private readonly IWarehouseRepository _warehouseRepository;

        public WarehousesController(IWarehouseRepository warehouseRepository)
        {
            _warehouseRepository = warehouseRepository;
        }

        // GET: Admin/Warehouse
        public async Task<IActionResult> Index()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            return View(warehouses);
        }

        // GET: Admin/Warehouse/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Warehouse/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Warehouse warehouse)
        {
            // <<< CẢI TIẾN 1: Thêm kiểm tra tên kho trùng lặp >>>
            // Giả sử bạn sẽ thêm GetByNameAsync vào repository
            /*
            var existingWarehouse = await _warehouseRepository.GetByNameAsync(warehouse.Name);
            if (existingWarehouse != null)
            {
                ModelState.AddModelError("Name", "Tên kho này đã tồn tại.");
            }
            */

            if (ModelState.IsValid)
            {
                // <<< CẢI TIẾN 2: Thêm tự động tạo Slug >>>
                warehouse.Slug = UrlHelper.GenerateSlug(warehouse.Name);
                await _warehouseRepository.AddAsync(warehouse);
                return RedirectToAction(nameof(Index));
            }
            return View(warehouse);
        }

        // GET: Admin/Warehouses/Edit/ten-kho
        [Route("Admin/Warehouses/Edit/{slug}")]
        public async Task<IActionResult> Edit(string slug)
        {
            var warehouse = await _warehouseRepository.GetBySlugAsync(slug);
            if (warehouse == null)
            {
                return NotFound();
            }
            return View(warehouse);
        }

        // POST: Admin/Warehouse/Edit/ten-kho
        // <<< BẮT ĐẦU KHỐI SỬA ĐỔI LỚN CHO ACTION EDIT (POST) >>>
        [HttpPost("Admin/Warehouses/Edit/{slug}")] // <<< THAY ĐỔI 1: Thêm route tường minh với slug
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string slug, Warehouse warehouse) // <<< THAY ĐỔI 2: Đổi tham số
        {
            var existingWarehouse = await _warehouseRepository.GetByIdAsync(warehouse.Id);
            if (existingWarehouse == null)
            {
                return NotFound();
            }

            if (slug != existingWarehouse.Slug)
            {
                return BadRequest();
            }

            ModelState.Remove("Slug");

            if (ModelState.IsValid)
            {
                try
                {
                    // <<< THAY ĐỔI 3: Áp dụng phương pháp cập nhật an toàn >>>
                    existingWarehouse.Name = warehouse.Name;
                    existingWarehouse.Slug = UrlHelper.GenerateSlug(warehouse.Name); // Tạo slug mới nếu tên thay đổi
                    // Cập nhật các trường khác nếu có, ví dụ:
                    // existingWarehouse.Address = warehouse.Address;

                    await _warehouseRepository.UpdateAsync(existingWarehouse);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _warehouseRepository.GetByIdAsync(warehouse.Id) == null)
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
            return View(warehouse);
        }
        // <<< KẾT THÚC KHỐI SỬA ĐỔI LỚN CHO ACTION EDIT (POST) >>>


        // GET: Admin/Warehouses/Delete/ten-kho
        [Route("Admin/Warehouses/Delete/{slug}")]
        public async Task<IActionResult> Delete(string slug)
        {
            var warehouse = await _warehouseRepository.GetBySlugAsync(slug);
            if (warehouse == null)
            {
                return NotFound();
            }
            return View(warehouse);
        }

        // POST: Admin/Warehouse/Delete/ten-kho
        // <<< BẮT ĐẦU KHỐI SỬA ĐỔI CHO ACTION DELETECONFIRMED (POST) >>>
        [HttpPost("Admin/Warehouses/Delete/{slug}"), ActionName("Delete")] // <<< THAY ĐỔI 1: Thêm route tường minh
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string slug) // <<< THAY ĐỔI 2: Đổi tham số
        {
            var warehouse = await _warehouseRepository.GetBySlugAsync(slug); // <<< THAY ĐỔI 3: Tìm bằng slug
            if (warehouse != null)
            {
                await _warehouseRepository.DeleteAsync(warehouse.Id); // <<< THAY ĐỔI 4: Xóa bằng Id
            }
            return RedirectToAction(nameof(Index));
        }
        // <<< KẾT THÚC KHỐI SỬA ĐỔI CHO ACTION DELETECONFIRMED (POST) >>>

        public async Task<IActionResult> UpdateAllWarehouseSlugs()
        {
            var warehouses = await _warehouseRepository.GetAllAsync();
            foreach (var warehouse in warehouses)
            {
                if (string.IsNullOrEmpty(warehouse.Slug))
                {
                    warehouse.Slug = UrlHelper.GenerateSlug(warehouse.Name);
                    await _warehouseRepository.UpdateAsync(warehouse);
                }
            }
            return Content("Đã cập nhật Slug cho tất cả các Kho.");
        }
    }
}