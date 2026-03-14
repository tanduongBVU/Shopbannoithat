using Microsoft.AspNetCore.Mvc;

namespace Shopbannoithat.Controllers
{
    public class OrderController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Checkout()
        {
            return View();
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
