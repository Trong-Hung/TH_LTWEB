using Microsoft.AspNetCore.Authorization; // Cần cho [Authorize]
using Microsoft.AspNetCore.Identity; // Cần để lấy User ID
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;

namespace VoTrongHung_2280601119.Controllers // Namespace này là gốc
{
    [Authorize] // Yêu cầu đăng nhập để xem lịch sử đơn hàng
    public class OrderController : Controller
    {
        private readonly IOrderDistributionRepository _orderDistributionRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderDistributionRepository orderDistributionRepository, UserManager<ApplicationUser> userManager)
        {
            _orderDistributionRepository = orderDistributionRepository;
            _userManager = userManager;
        }

        // GET: /Order/History (Lịch sử Đơn phân phối cho Customer)
        public async Task<IActionResult> History()
        {
            var userId = _userManager.GetUserId(User); // Lấy ID của người dùng hiện tại
            var orders = (await _orderDistributionRepository.GetAllAsync())
                         .Where(o => o.CustomerId == userId)
                         .OrderByDescending(o => o.OrderDate)
                         .ToList();
            return View(orders);
        }
    }
}