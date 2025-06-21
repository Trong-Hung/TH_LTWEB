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
            if (ModelState.IsValid)
            {
                await _warehouseRepository.AddAsync(warehouse);
                return RedirectToAction(nameof(Index));
            }
            return View(warehouse);
        }

        /// GET: Admin/Warehouses/Edit/ten-kho
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

        // POST: Admin/Warehouse/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Warehouse warehouse)
        {
            if (id != warehouse.Id) { return NotFound(); }

            if (ModelState.IsValid)
            {
                try
                {
                    await _warehouseRepository.UpdateAsync(warehouse);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await _warehouseRepository.GetByIdAsync(warehouse.Id) == null) { return NotFound(); }
                    else { throw; }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(warehouse);
        }

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

        // POST: Admin/Warehouse/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var warehouse = await _warehouseRepository.GetByIdAsync(id);
            if (warehouse != null)
            {
                await _warehouseRepository.DeleteAsync(id);
            }
            return RedirectToAction(nameof(Index));
        }

        // Thêm Action này vào WarehousesController.cs

        public async Task<IActionResult> UpdateAllWarehouseSlugs()
        {
            // Lấy tất cả các kho từ database
            var warehouses = await _warehouseRepository.GetAllAsync();

            // Duyệt qua từng kho
            foreach (var warehouse in warehouses)
            {
                // Nếu kho nào chưa có slug
                if (string.IsNullOrEmpty(warehouse.Slug))
                {
                    // Tạo slug mới từ tên của kho
                    warehouse.Slug = UrlHelper.GenerateSlug(warehouse.Name);

                    // Cập nhật lại thông tin kho đó trong database
                    await _warehouseRepository.UpdateAsync(warehouse);
                }
            }

            // Trả về một thông báo đơn giản để xác nhận đã hoàn thành
            return Content("Đã cập nhật Slug cho tất cả các Kho.");
        }
    }
}