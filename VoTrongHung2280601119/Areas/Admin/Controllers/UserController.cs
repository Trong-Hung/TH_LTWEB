using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;

namespace VoTrongHung_2280601119.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        // GET: Admin/User (Danh sách người dùng)
        public async Task<IActionResult> Index()
        {
            // Lấy tất cả người dùng, trừ admin@dist.com
            var users = await _userManager.Users.Where(u => u.Email != "admin@dist.com").ToListAsync();
            var customers = await _userManager.GetUsersInRoleAsync("Customer");
            ViewData["Title"] = "Danh sách Khách hàng";
            return View(users);
        }


        // POST: Admin/User/ToggleStatus/userId (Enable/Disable người dùng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) { return NotFound(); }

            // Đảm bảo không khóa tài khoản Admin gốc
            if (user.Email == "admin@dist.com")
            {
                TempData["Error"] = "Không thể vô hiệu hóa tài khoản Admin gốc.";
                return RedirectToAction(nameof(Index));
            }

            // LockoutEnd = null hoặc trong quá khứ => tài khoản đang được kích hoạt
            if (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow)
            {
                // Vô hiệu hóa tài khoản (khóa tài khoản đến một ngày rất xa trong tương lai)
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            }
            else // Tài khoản đang bị vô hiệu hóa (LockoutEnd trong tương lai)
            {
                // Kích hoạt tài khoản (thiết lập LockoutEnd về null)
                await _userManager.SetLockoutEndDateAsync(user, null);
            }
            // Cập nhật lại user trong DB
            await _userManager.UpdateAsync(user);

            return RedirectToAction(nameof(Index));
        }
    }
}