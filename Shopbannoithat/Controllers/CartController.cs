using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
                return RedirectToAction("Login", "Auth");

            var cart = _context.Carts
                .Include(x => x.Product)
                .Where(x => x.UserEmail == email)
                .ToList();

            return View(cart);
        }

        // Thêm sản phẩm
        public IActionResult Add(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
                return RedirectToAction("Login", "Auth");

            var product = _context.Products.Find(id);

            if (product == null || product.Quantity <= 0)
            {
                TempData["Error"] = "Sản phẩm đã hết hàng!";
                return RedirectToAction("Index", "Product");
            }

            var cart = _context.Carts
                .FirstOrDefault(x => x.ProductId == id && x.UserEmail == email);

            if (cart != null)
            {
                if (cart.Quantity + 1 > product.Quantity)
                {
                    TempData["Error"] = "Không đủ hàng trong kho!";
                    return RedirectToAction("Index");
                }

                cart.Quantity++;
            }
            else
            {
                cart = new Cart
                {
                    ProductId = id,
                    UserEmail = email,
                    Quantity = 1
                };

                _context.Carts.Add(cart);
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // Xóa sản phẩm
        public IActionResult Remove(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            var cart = _context.Carts
                .FirstOrDefault(x => x.ProductId == id && x.UserEmail == email);

            if (cart != null)
            {
                _context.Carts.Remove(cart);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult GetCartCount()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
                return Json(0);

            var count = _context.Carts
                .Where(x => x.UserEmail == email)
                .Sum(x => x.Quantity);

            return Json(count);
        }

        public IActionResult Increase(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            var cart = _context.Carts
                .Include(x => x.Product)
                .FirstOrDefault(x => x.ProductId == id && x.UserEmail == email);

            if (cart != null)
            {
                if (cart.Quantity + 1 > cart.Product.Quantity)
                {
                    TempData["Error"] = "Không đủ hàng trong kho!";
                    return RedirectToAction("Index");
                }

                cart.Quantity++;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Decrease(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            var cart = _context.Carts
                .FirstOrDefault(x => x.ProductId == id && x.UserEmail == email);

            if (cart != null)
            {
                cart.Quantity--;

                if (cart.Quantity <= 0)
                {
                    _context.Carts.Remove(cart);
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        

        

        public IActionResult AddToCart(int id)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
                return RedirectToAction("Login", "Auth");

            var cart = _context.Carts
                .FirstOrDefault(x => x.ProductId == id && x.UserEmail == email);

            if (cart != null)
            {
                cart.Quantity++;
            }
            else
            {
                cart = new Cart
                {
                    ProductId = id,
                    UserEmail = email,
                    Quantity = 1
                };

                _context.Carts.Add(cart);
            }

            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
