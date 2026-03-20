using Microsoft.AspNetCore.SignalR;
using Shopbannoithat.Data;
using Shopbannoithat.Models;
using Shopbannoithat.Services;

namespace Shopbannoithat.Hubs
{
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _context;
        private readonly GeminiService _gemini;

        public ChatHub(ApplicationDbContext context, GeminiService gemini)
        {
            _context = context;
            _gemini = gemini;
        }

        public async Task SendMessageToAdmin(string message)
        {
            var email = Context.GetHttpContext()?.Session.GetString("UserEmail");
            var role = Context.GetHttpContext()?.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(email)) return;

            // Lưu tin nhắn khách vào DB
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

            // Gửi tin khách lên Admin/Staff
            await Clients.Group("AdminStaff").SendAsync("ReceiveMessage", email, role, message, email, chatMsg.SentAt.ToString("HH:mm"));

            // Gửi lại cho khách thấy tin của mình
            await Clients.Caller.SendAsync("ReceiveMessage", email, role, message, email, chatMsg.SentAt.ToString("HH:mm"));

            // AI tự động trả lời
            try
            {
                var aiResult = await _gemini.AskAsync(message);
                var aiTime = DateTime.Now;

                // Tạo message kèm cards JSON
                var aiPayload = System.Text.Json.JsonSerializer.Serialize(new
                {
                    text = aiResult.Message,
                    cards = aiResult.Cards
                });

                var aiMsg = new ChatMessage
                {
                    SenderEmail = "AI ShopNoiThat",
                    SenderRole = "Staff",
                    Message = aiResult.Message,
                    CustomerEmail = email,
                    SentAt = aiTime,
                    CardsJson = aiResult.Cards.Any()
                   ? System.Text.Json.JsonSerializer.Serialize(aiResult.Cards)
                   : null
                };
                _context.ChatMessages.Add(aiMsg);
                await _context.SaveChangesAsync();

                await Clients.Group($"customer_{email}").SendAsync("ReceiveAIMessage",
                    aiResult.Message, aiResult.Cards, aiTime.ToString("HH:mm"));
                await Clients.Group("AdminStaff").SendAsync("ReceiveMessage",
                    "🤖 AI ShopNoiThat", "Staff", aiResult.Message, email, aiTime.ToString("HH:mm"));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gemini error: " + ex.Message);
                await Clients.Caller.SendAsync("ReceiveMessage", "AI ShopNoiThat", "Staff",
                    "Xin lỗi, trợ lý AI đang bận. Nhân viên sẽ hỗ trợ bạn sớm!",
                    email, DateTime.Now.ToString("HH:mm"));
            }
        }

        // Giữ nguyên ReplyToCustomer và OnConnectedAsync
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

            await Clients.Group($"customer_{customerEmail}").SendAsync("ReceiveMessage", senderEmail, senderRole, message, customerEmail, chatMsg.SentAt.ToString("HH:mm"));
            await Clients.Group("AdminStaff").SendAsync("ReceiveMessage", senderEmail, senderRole, message, customerEmail, chatMsg.SentAt.ToString("HH:mm"));
        }

        public override async Task OnConnectedAsync()
        {
            var role = Context.GetHttpContext()?.Session.GetString("UserRole");
            var email = Context.GetHttpContext()?.Session.GetString("UserEmail");

            if (role == "Admin" || role == "Staff")
                await Groups.AddToGroupAsync(Context.ConnectionId, "AdminStaff");
            else if (!string.IsNullOrEmpty(email))
                await Groups.AddToGroupAsync(Context.ConnectionId, $"customer_{email}");

            await base.OnConnectedAsync();
        }
    }
}