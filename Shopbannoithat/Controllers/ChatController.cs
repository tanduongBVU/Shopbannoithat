using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;

namespace Shopbannoithat.Controllers
{
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // API lấy lịch sử chat của customer đang đăng nhập
        public IActionResult GetHistory()
        {
            var email = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(email))
                return Json(new List<object>());

            var messages = _context.ChatMessages
                .Where(m => m.CustomerEmail == email)
                .OrderBy(m => m.SentAt)
                .Select(m => new {
                    m.SenderEmail,
                    m.SenderRole,
                    m.Message,
                    m.CardsJson,
                    SentAt = m.SentAt.ToString("HH:mm dd/MM")
                })
                .ToList();

            return Json(messages);
        }
    }
}