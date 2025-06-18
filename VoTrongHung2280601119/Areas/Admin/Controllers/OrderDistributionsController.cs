using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;

namespace VoTrongHung_2280601119.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly IOrderDistributionRepository _orderDistributionRepository;

        public OrderController(IOrderDistributionRepository orderDistributionRepository)
        {
            _orderDistributionRepository = orderDistributionRepository;
        }

        // GET: Admin/Order (Danh sách đơn hàng)
        public async Task<IActionResult> Index()
        {
            var orders = await _orderDistributionRepository.GetAllAsync();
            return View(orders);
        }

        // POST: Admin/Order/Confirm/5 (Xác nhận đơn hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var order = await _orderDistributionRepository.GetByIdAsync(id);
            if (order == null) { return NotFound(); }

            order.Status = "Đã Xác nhận"; // Cập nhật trạng thái
            await _orderDistributionRepository.UpdateAsync(order);
            // TODO: Cập nhật tồn kho sản phẩm nếu cần thiết khi xác nhận đơn hàng
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Order/Cancel/5 (Hủy đơn hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _orderDistributionRepository.GetByIdAsync(id);
            if (order == null) { return NotFound(); }

            order.Status = "Đã Hủy"; // Cập nhật trạng thái
            await _orderDistributionRepository.UpdateAsync(order);
            // TODO: Hoàn lại tồn kho sản phẩm nếu đã trừ khi đặt hàng
            return RedirectToAction(nameof(Index));
        }
    }
}