using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;
using Shopbannoithat.Models;
using System.Text.Json;

namespace Shopbannoithat.Controllers
{
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WishlistController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Add(int id)
        {
            var product = _context.Products.Find(id);

            var wishlist = HttpContext.Session.GetString("wishlist");

            List<WishlistItem> items;

            if (wishlist == null)
            {
                items = new List<WishlistItem>();
            }
            else
            {
                items = JsonSerializer.Deserialize<List<WishlistItem>>(wishlist);
            }

            if (!items.Any(p => p.ProductId == id))
            {
                items.Add(new WishlistItem
                {
                    ProductId = product.Id,
                    Name = product.Name,
                    Price = (decimal)product.Price,
                    Image = product.Image
                });
            }

            HttpContext.Session.SetString("wishlist", JsonSerializer.Serialize(items));

            return Json(new { count = items.Count });
        }

        public IActionResult Index()
        {
            var wishlist = HttpContext.Session.GetString("wishlist");

            if (wishlist == null)
                return View(new List<WishlistItem>());

            var items = JsonSerializer.Deserialize<List<WishlistItem>>(wishlist);

            return View(items);
        }
    }
}