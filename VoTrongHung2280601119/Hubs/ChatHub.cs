// File: ~/Hubs/ChatHub.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Services; // Thêm namespace này

namespace VoTrongHung2280601119.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager; // Thêm UserManager
        private readonly IUserConnectionManager _userConnectionManager; // Thêm IUserConnectionManager

        public ChatHub(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IUserConnectionManager userConnectionManager)
        {
            _context = context;
            _userManager = userManager; // Khởi tạo
            _userConnectionManager = userConnectionManager; // Khởi tạo
        }

        // Gửi tin nhắn đến một phòng cụ thể (dùng cho cả chung và riêng)
        public async Task SendMessage(string message, string roomName)
        {
            var userName = Context.User.Identity?.Name;
            var userId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(roomName))
                return;

            var chatMessage = new Message
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                UserName = userName,
                UserId = userId,
                RoomName = roomName
            };

            _context.Messages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Gửi tin nhắn đến tất cả client trong phòng đó
            await Clients.Group(roomName).SendAsync("ReceiveMessage", userName, message, roomName);

            // Nếu đây là phòng hỗ trợ, gửi thông báo đặc biệt đến tất cả Admin
            if (roomName != "General")
            {
                // Logic để lấy tên khách hàng nếu người gửi là khách hàng
                var senderUser = await _userManager.FindByIdAsync(userId);
                // Lấy tên của khách hàng (người gửi tin nhắn)
                var customerName = senderUser?.UserName ?? "Khách hàng mới";

                // Chỉ gửi thông báo nếu tin nhắn đến từ một khách hàng và đang chat với admin
                // Dựa vào cấu trúc roomName (customerId-adminId) và vai trò của người gửi
                if (Context.User.IsInRole("ROLE_CUSTOMER")) // Nếu người gửi là khách hàng
                {
                    await Clients.Group("Admins").SendAsync("NewSupportConversation", customerName, roomName);
                }
            }
        }

        // THÊM: Phương thức để khách hàng gửi tin nhắn riêng đến Admin
        public async Task SendPrivateMessage(string adminId, string message)
        {
            var senderUserName = Context.User.Identity?.Name;
            var senderUserId = Context.UserIdentifier;

            if (string.IsNullOrEmpty(message) || string.IsNullOrEmpty(senderUserName) || string.IsNullOrEmpty(adminId))
                return;

            // Đảm bảo adminId và senderUserId luôn sắp xếp theo cùng một thứ tự để tạo roomName ổn định
            var roomName = string.CompareOrdinal(senderUserId, adminId) < 0
                ? $"{senderUserId}-{adminId}"
                : $"{adminId}-{senderUserId}";

            var chatMessage = new Message
            {
                Content = message,
                Timestamp = DateTime.UtcNow,
                UserName = senderUserName,
                UserId = senderUserId,
                RoomName = roomName
            };

            _context.Messages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // Gửi tin nhắn đến chính người gửi (khách hàng)
            await Clients.Caller.SendAsync("ReceivePrivateMessage", senderUserId, message);

            // Lấy tất cả connection IDs của admin
            var adminConnections = _userConnectionManager.GetUserConnections(adminId);
            if (adminConnections != null && adminConnections.Any())
            {
                // Gửi tin nhắn đến tất cả các connection của admin trong phòng đó
                // Có thể cải thiện: chỉ gửi đến admin nếu admin đang trong phòng đó
                // Hiện tại đang gửi tới tất cả các connection của adminId, sau đó ở client-side Admin mới lọc theo roomName
                // Hoặc bạn có thể gửi trực tiếp vào Group(roomName) nếu muốn
                await Clients.Group(roomName).SendAsync("ReceiveMessage", senderUserName, message, roomName);
            }

            // Gửi thông báo đến nhóm Admin để họ biết có cuộc trò chuyện mới
            if (Context.User.IsInRole("ROLE_CUSTOMER"))
            {
                await Clients.Group("Admins").SendAsync("NewSupportConversation", senderUserName, roomName);
            }
        }


        // THÊM: Phương thức để rời phòng
        public async Task LeaveRoom(string roomName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomName);
        }

        // Tham gia vào một phòng
        public async Task JoinRoom(string roomName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        }

        // Admin sẽ gọi phương thức này khi kết nối để nhận thông báo
        public async Task JoinAdminGroup()
        {
            if (Context.User.IsInRole("ROLE_ADMIN"))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
            }
        }

        // Thêm vào các phương thức OnConnectedAsync và OnDisconnectedAsync để quản lý UserConnectionManager
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(userId))
            {
                _userConnectionManager.AddUserConnection(userId, Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            _userConnectionManager.RemoveUserConnection(connectionId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}