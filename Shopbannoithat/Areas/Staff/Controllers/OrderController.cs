using Microsoft.AspNetCore.Mvc;

namespace Shopbannoithat.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            // chưa đăng nhập hoặc không phải Staff
            if (string.IsNullOrEmpty(role) || role != "Staff")
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            return View();
        }
    }
}
