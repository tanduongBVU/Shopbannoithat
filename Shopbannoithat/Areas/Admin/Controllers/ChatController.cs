using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbannoithat.Data;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ChatController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChatController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Admin" && role != "Staff")
                return RedirectToAction("Login", "Auth", new { area = "" });

            // Lấy danh sách khách đã nhắn
            var customers = _context.ChatMessages
                .Where(m => m.SenderRole == "Customer")
                .Select(m => m.CustomerEmail)
                .Distinct()
                .ToList();

            return View(customers);
        }

        // API lấy lịch sử chat của 1 khách
        public IActionResult GetHistory(string customerEmail)
        {
            var messages = _context.ChatMessages
                .Where(m => m.CustomerEmail == customerEmail)
                .OrderBy(m => m.SentAt)
                .Select(m => new {
                    m.SenderEmail,
                    m.SenderRole,
                    m.Message,
                    SentAt = m.SentAt.ToString("HH:mm dd/MM")
                })
                .ToList();

            return Json(messages);
        }
    }
}