using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbannoithat.Data;
using Shopbannoithat.Models;
using System.Security.Claims;
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
            var userId = HttpContext.Session.GetString("UserEmail");

            if (userId == null)
                return Json(new { success = false });

            var product = _context.Products.Find(id);

            var exist = _context.Wishlists
                .FirstOrDefault(w => w.ProductId == id && w.UserId == userId);

            bool isAdded;

            if (exist == null)
            {
                // Chưa có → thêm vào
                var item = new WishlistItem
                {
                    ProductId = product.Id,
                    UserId = userId,
                    Name = product.Name,
                    Price = product.Price ?? 0,
                    Image = product.Image
                };
                _context.Wishlists.Add(item);
                isAdded = true;
            }
            else
            {
                // Đã có → xóa ra
                _context.Wishlists.Remove(exist);
                isAdded = false;
            }

            _context.SaveChanges();

            var count = _context.Wishlists.Count(w => w.UserId == userId);

            return Json(new { success = true, count = count, isAdded = isAdded });
        }

        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetString("UserEmail");

            if (userId == null)
                return RedirectToAction("Login", "Auth");

            var items = _context.Wishlists
                .Include(w => w.Product)   // thêm dòng này
                .Where(w => w.UserId == userId)
                .ToList();

            return View(items);
        }

        public IActionResult Count()
        {
            var userId = HttpContext.Session.GetString("UserEmail");

            if (userId == null)
                return Json(0);

            var count = _context.Wishlists.Count(w => w.UserId == userId);

            return Json(count);
        }

        public IActionResult Remove(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            var item = _context.Wishlists
                .FirstOrDefault(x => x.ProductId == id && x.UserId == email);

            if (item != null)
            {
                _context.Wishlists.Remove(item);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}