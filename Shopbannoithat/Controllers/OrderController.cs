using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang thanh toán
        public IActionResult Checkout()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
                return RedirectToAction("Login", "Auth");

            var cartItems = _context.Carts
                .Include(x => x.Product)
                .Where(x => x.UserEmail == email)
                .ToList();

            return View(cartItems);
        }

        // Xác nhận đặt hàng
        [HttpPost]
        public IActionResult PlaceOrder(Order order)
        {
            var email = HttpContext.Session.GetString("UserEmail");

            var cartItems = _context.Carts
                .Include(x => x.Product)
                .Where(x => x.UserEmail == email)
                .ToList();

            if (!cartItems.Any())
                return RedirectToAction("Checkout");

            order.UserId = email;
            order.OrderDate = DateTime.Now;
            order.Status = "Đang xử lý";
            order.OrderPlaced = DateTime.Now;

            order.TotalPrice = cartItems.Sum(x => (x.Product.Price ?? 0) * x.Quantity);

            order.OrderDetails = new List<OrderDetail>();

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

        // Trang đặt hàng thành công
        public IActionResult Success(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefault(o => o.Id == id);

            return View(order);
        }
    }
}