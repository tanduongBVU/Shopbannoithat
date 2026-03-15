using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var products = _context.Products.ToList();

            return View(products);
        }

        public IActionResult UpdateStock(int id, int quantity)
        {
            var product = _context.Products.Find(id);

            if (product != null)
            {
                product.Quantity += quantity;

                if (product.Quantity < 0)
                    product.Quantity = 0;

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
