using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoTrongHung2280601119.Models;

namespace VoTrongHung2280601119.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ROLE_ADMIN")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var userCount = await _context.Users.CountAsync();
            var productCount = await _context.Products.CountAsync();
            var orderCount = await _context.OrderDistributions.CountAsync();
            var pendingOrders = await _context.OrderDistributions.CountAsync(o => o.Status == "Đang xử lý");

            // Tổng doanh thu từ các đơn hàng đã hoàn tất
            var totalRevenue = await _context.OrderDistributions
            .Where(o => o.Status == "Đã Xác nhận")  // nếu bạn chỉ dùng trạng thái này
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            // Doanh thu theo từng ngày trong tháng hiện tại
            var today = DateTime.Today;
            var startDate = new DateTime(today.Year, today.Month, 1);
            var endDate = startDate.AddMonths(1);

            var dailyRevenue = await _context.OrderDistributions
                .Where(o => o.Status == "Đã Xác nhận" && o.OrderDate >= startDate && o.OrderDate < endDate)
                .GroupBy(o => o.OrderDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.TotalPrice)
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Truyền dữ liệu sang View
            ViewData["UserCount"] = userCount;
            ViewData["ProductCount"] = productCount;
            ViewData["OrderCount"] = orderCount;
            ViewData["PendingOrders"] = pendingOrders;
            ViewData["TotalRevenue"] = totalRevenue;

            ViewBag.ChartLabels = dailyRevenue.Select(d => d.Date.ToString("dd/MM")).ToList();
            ViewBag.ChartData = dailyRevenue.Select(d => d.Revenue).ToList();

            return View();
        }
    }
}
