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
                .FirstOrDefault(x => x.ProductId == id && x.UserEmail == email);

            if (cart != null)
            {
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

        public IActionResult Checkout()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
                return RedirectToAction("Login", "Auth");

            var cartItems = _context.Carts
                .Include(x => x.Product)
                .Where(x => x.UserEmail == email)
                .ToList();

            if (!cartItems.Any())
                return RedirectToAction("Index");

            var order = new Order
            {
                UserId = email,
                OrderDate = DateTime.Now,
                Status = "Đang xử lý",
                TotalPrice = cartItems.Sum(x => (x.Product.Price ?? 0) * x.Quantity),
                OrderDetails = new List<OrderDetail>()
            };

            foreach (var item in cartItems)
            {
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price ?? 0
                });
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            _context.Carts.RemoveRange(cartItems);
            _context.SaveChanges();

            return RedirectToAction("Success", new { id = order.Id });
        }

        public IActionResult Success(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);

            return View(order);
        }

        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.Find(id);

            var cartItem = new Cart
            {
                ProductId = id,
                Quantity = 1
            };

            _context.Carts.Add(cartItem);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
