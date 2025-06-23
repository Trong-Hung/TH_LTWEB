using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoTrongHung2280601119.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VoTrongHung2280601119.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ROLE_ADMIN")]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var conversations = new List<ConversationViewModel>();
            var admins = await _userManager.GetUsersInRoleAsync("ROLE_ADMIN");
            var adminIds = admins.Select(a => a.Id).ToList();

            var roomGroups = await _context.Messages
                .Where(m => m.RoomName != "General" && m.RoomName.Contains("-"))
                .GroupBy(m => m.RoomName)
                .Select(g => new {
                    RoomName = g.Key,
                    LastMessageTimestamp = g.Max(m => m.Timestamp)
                })
                .OrderByDescending(x => x.LastMessageTimestamp)
                .ToListAsync();

            foreach (var roomInfo in roomGroups)
            {
                var userIds = roomInfo.RoomName.Split('-');
                if (userIds.Length != 2) continue;
                var customerId = userIds.FirstOrDefault(id => !adminIds.Contains(id));

                if (!string.IsNullOrEmpty(customerId))
                {
                    var customer = await _userManager.FindByIdAsync(customerId);
                    if (customer != null)
                    {
                        conversations.Add(new ConversationViewModel
                        {
                            RoomName = roomInfo.RoomName,
                            CustomerName = customer.UserName ?? "Không rõ"
                        });
                    }
                }
            }
            return View(conversations);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessages(string roomName)
        {
            var messages = await _context.Messages
                .Where(m => m.RoomName == roomName)
                .OrderBy(m => m.Timestamp)
                .Select(m => new { user = m.UserName, message = m.Content })
                .ToListAsync();
            return Json(messages);
        }
    }
}
