using Microsoft.AspNetCore.Mvc;

namespace Shopbannoithat.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Staff")
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            return View();
        }
    }
}
