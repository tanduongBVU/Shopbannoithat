using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;

namespace Shopbannoithat.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class AttributeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AttributeController(ApplicationDbContext context) => _context = context;

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            // chưa đăng nhập hoặc không phải Staff
            if (string.IsNullOrEmpty(role) || role != "Staff")
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            ViewBag.Sizes = _context.Sizes.ToList();
            ViewBag.Materials = _context.Materials.ToList();
            ViewBag.Colors = _context.Colors.ToList();
            return View();
        }
    }
}
