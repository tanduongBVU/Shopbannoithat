using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;

namespace Shopbannoithat.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CustomerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Staff")
                return RedirectToAction("Login", "Auth", new { area = "" });

            var customers = _context.Users
                .Where(u => u.Role == "Customer")
                .ToList();

            return View(customers);
        }
    }
}
