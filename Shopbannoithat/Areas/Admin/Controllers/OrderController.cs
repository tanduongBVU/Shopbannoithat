using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;
using Microsoft.EntityFrameworkCore;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            // chưa đăng nhập hoặc không phải Staff
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var orders = _context.Orders
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        public IActionResult UpdateStatus(int id, string status)
        {
            var order = _context.Orders.Find(id);

            if (order != null)
            {
                order.Status = status;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Detail(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(role) || role != "Admin")
                return RedirectToAction("Login", "Auth", new { area = "" });

            var order = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Product)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Variant)
                        .ThenInclude(v => v.Size)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Variant)
                        .ThenInclude(v => v.Material)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Variant)
                        .ThenInclude(v => v.Color)
                .FirstOrDefault(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }
    }
}
