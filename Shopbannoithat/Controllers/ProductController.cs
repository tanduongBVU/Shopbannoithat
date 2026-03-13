using Microsoft.AspNetCore.Mvc;

namespace Shopbannoithat.Controllers
{
    public class ProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
