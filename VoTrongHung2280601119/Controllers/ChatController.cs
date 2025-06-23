using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoTrongHung2280601119.Models;
using System.Linq;
using System.Threading.Tasks;

namespace VoTrongHung2280601119.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Phòng chat chung
        public async Task<IActionResult> Index()
        {
            ViewData["RoomName"] = "General";
            var messages = await _context.Messages.Where(m => m.RoomName == "General").OrderBy(m => m.Timestamp).Take(100).ToListAsync();
            return View(messages);
        }

        // Hỗ trợ trực tuyến
        public async Task<IActionResult> Support()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var admin = (await _userManager.GetUsersInRoleAsync("ROLE_ADMIN")).FirstOrDefault();

            if (admin == null || currentUser == null)
            {
                TempData["Error"] = "Hệ thống hỗ trợ đang tạm thời gián đoạn.";
                return RedirectToAction("Index", "Home");
            }

            // Tạo tên phòng riêng duy nhất và ổn định
            var roomName = string.CompareOrdinal(currentUser.Id, admin.Id) < 0
                ? $"{currentUser.Id}-{admin.Id}"
                : $"{admin.Id}-{currentUser.Id}";

            ViewData["RoomName"] = roomName;
            var messages = await _context.Messages.Where(m => m.RoomName == roomName).OrderBy(m => m.Timestamp).Take(100).ToListAsync();
            return View(messages);
        }

        // MỚI THÊM: Action này để widget chat trên _Layout.cshtml lấy lịch sử tin nhắn dạng JSON
        [HttpGet]
        public async Task<IActionResult> GetMessagesWidget(string roomName)
        {
            if (string.IsNullOrEmpty(roomName))
            {
                return BadRequest("Room name is required.");
            }

            // Đảm bảo chỉ người dùng trong phòng đó (hoặc admin) mới có thể xem tin nhắn
            // Lấy ID của người dùng hiện tại
            var currentUserId = _userManager.GetUserId(User);

            // Tách các ID từ roomName
            var userIdsInRoom = roomName.Split('-');

            // Kiểm tra xem người dùng hiện tại có phải là một phần của roomName này không
            // Hoặc nếu người dùng hiện tại là ADMIN, họ có quyền xem tất cả các cuộc trò chuyện support
            var isAdmin = await _userManager.IsInRoleAsync(await _userManager.GetUserAsync(User), "ROLE_ADMIN");

            if (!isAdmin && !userIdsInRoom.Contains(currentUserId))
            {
                // Nếu không phải admin và không phải là một phần của phòng, từ chối truy cập
                return Forbid();
            }

            var messages = await _context.Messages
                .Where(m => m.RoomName == roomName)
                .OrderBy(m => m.Timestamp)
                .Take(100) // Giới hạn số lượng tin nhắn để tránh tải quá nhiều
                .Select(m => new { user = m.UserName, message = m.Content }) // Chỉ lấy User và Message
                .ToListAsync();

            return Json(messages);
        }
    }
}