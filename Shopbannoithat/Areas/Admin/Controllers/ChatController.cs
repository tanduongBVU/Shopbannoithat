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
            var role = HttpContext.Session.GetString("UserRole");  /*lấy userRole đã lưu trong session*/
            if (role != "Admin" && role != "Staff")  /*nếu không phải admin và staff*/ 
                return RedirectToAction("Login", "Auth", new { area = "" });  /*chuyển qua trang đăng nhập*/

            // Lấy danh sách khách đã nhắn
            var customers = _context.ChatMessages
                .Where(m => m.SenderRole == "Customer")  /*Lọc những tin nhắn có SenderRole = "Customer" tức là chỉ lấy tin nhắn do khách hàng gửi*/
                .Select(m => m.CustomerEmail)  /*là lấy email khách hàng từ tin nhắn*/
                .Distinct() /*Loại bỏ những email bị trùng*/
                .ToList();

            return View(customers);
        }

        // API lấy lịch sử chat của 1 khách
        public IActionResult GetHistory(string customerEmail)
        {
            var messages = _context.ChatMessages
                .Where(m => m.CustomerEmail == customerEmail)  /*lọc ra email khách hàng cần tìm */
                .OrderBy(m => m.SentAt)  /*sắp xếp tin nhắn theo thời gian gửi*/
                .Select(m => new {   /*Tạo object mới chỉ chứa những cột cần dùng*/
                    m.SenderEmail,    /*email người gửi*/
                    m.SenderRole,     /*role ngưởi gửi*/
                    m.Message,        /*tin nhắn*/
                    SentAt = m.SentAt.ToString("HH:mm dd/MM")   /*chuyển tg từ datetime sang chuỗi*/
                })
                .ToList();

            return Json(messages);      /*trả dữ liệu dạng json cho javascript sửa dụng*/
        }
    }
}