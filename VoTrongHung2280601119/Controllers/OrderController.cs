using Microsoft.AspNetCore.Authorization; // Cần cho [Authorize]
using Microsoft.AspNetCore.Identity; // Cần để lấy User ID
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;


        public OrderController(IOrderDistributionRepository orderDistributionRepository, UserManager<ApplicationUser> userManager, ApplicationDbContext db, IEmailSender emailSender)
        {
            _orderDistributionRepository = orderDistributionRepository;
            _userManager = userManager;
            _db = db;
            _emailSender = emailSender; // GÁN

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

        // Gửi email xác nhận đơn hàng
        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var user = await _userManager.GetUserAsync(User);

            // Tạo đơn hàng mới (giả định)
            var order = new OrderDistribution
            {
                CustomerId = user.Id,
                OrderDate = DateTime.Now,
                Status = "Đang xử lý"
            };
            _db.OrderDistributions.Add(order); 
            await _db.SaveChangesAsync();

            // 📧 Gửi email xác nhận đơn hàng
            string subject = "Xác nhận đơn hàng #" + order.Id;
            string body = $@"
            <h2>Xin chào {user.FullName},</h2>
            <p>Cảm ơn bạn đã đặt hàng tại hệ thống của chúng tôi.</p>
            <p>Mã đơn hàng của bạn là <strong>{order.Id}</strong>.</p>
            <p>Trạng thái hiện tại: <strong>{order.Status}</strong>.</p>
            <br/>
            <p>Chúng tôi sẽ liên hệ với bạn khi đơn hàng được giao.</p>
            <p>Trân trọng,</p>
            <p><em>Hệ thống phân phối sản phẩm</em></p>";

            await _emailSender.SendEmailAsync(user.Email, subject, body);

            return RedirectToAction("OrderSuccess", new { id = order.Id });
        }
    }
}