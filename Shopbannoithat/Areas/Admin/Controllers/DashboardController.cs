using Microsoft.AspNetCore.Mvc;


namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    public class DashboardController : Controller
    {

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (role != "Admin")
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            return View();
        }
    }
}
