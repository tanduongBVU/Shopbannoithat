using Microsoft.AspNetCore.SignalR;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;

        public ChatHub(ApplicationDbContext context)
        {
            _context = context;
        }

        // Khách hàng gửi tin
        public async Task SendMessageToAdmin(string message)
        {
            var email = Context.GetHttpContext()?.Session.GetString("UserEmail");
            var role = Context.GetHttpContext()?.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(email)) return;

            var chatMsg = new ChatMessage
            {
                SenderEmail = email,
                SenderRole = role ?? "Customer",
                Message = message,
                CustomerEmail = email,
                SentAt = DateTime.Now
            };

            _context.ChatMessages.Add(chatMsg);
            await _context.SaveChangesAsync();

            // Gửi tới tất cả Admin/Staff đang online
            await Clients.Group("AdminStaff").SendAsync("ReceiveMessage", email, role, message, email, chatMsg.SentAt.ToString("HH:mm"));

            // Gửi lại cho chính khách để hiển thị tin của mình
            await Clients.Caller.SendAsync("ReceiveMessage", email, role, message, email, chatMsg.SentAt.ToString("HH:mm"));
        }

        // Admin/Staff reply cho khách
        public async Task ReplyToCustomer(string customerEmail, string message)
        {
            var senderEmail = Context.GetHttpContext()?.Session.GetString("UserEmail");
            var senderRole = Context.GetHttpContext()?.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(senderEmail)) return;
            if (senderRole != "Admin" && senderRole != "Staff") return;

            var chatMsg = new ChatMessage
            {
                SenderEmail = senderEmail,
                SenderRole = senderRole,
                Message = message,
                CustomerEmail = customerEmail,
                SentAt = DateTime.Now
            };

            _context.ChatMessages.Add(chatMsg);
            await _context.SaveChangesAsync();

            // Gửi tới nhóm của khách đó
            await Clients.Group($"customer_{customerEmail}").SendAsync("ReceiveMessage", senderEmail, senderRole, message, customerEmail, chatMsg.SentAt.ToString("HH:mm"));

            // Gửi tới tất cả Admin/Staff thấy reply
            await Clients.Group("AdminStaff").SendAsync("ReceiveMessage", senderEmail, senderRole, message, customerEmail, chatMsg.SentAt.ToString("HH:mm"));
        }

        // Khi kết nối, tự join group theo role
        public override async Task OnConnectedAsync()
        {
            var role = Context.GetHttpContext()?.Session.GetString("UserRole");
            var email = Context.GetHttpContext()?.Session.GetString("UserEmail");

            if (role == "Admin" || role == "Staff")
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, "AdminStaff");
            }
            else if (!string.IsNullOrEmpty(email))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"customer_{email}");
            }

            await base.OnConnectedAsync();
        }
    }
}