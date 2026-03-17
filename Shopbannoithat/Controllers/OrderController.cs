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
        public IActionResult PlaceOrder(Order order, string CouponCode)
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

            decimal total = cartItems.Sum(x => (x.Product.Price ?? 0) * x.Quantity);

            Coupon coupon = null; // 🔥 giữ lại để dùng phía dưới

            // =========================
            // 🔥 XỬ LÝ MÃ GIẢM GIÁ
            // =========================
            if (!string.IsNullOrEmpty(CouponCode))
            {
                coupon = _context.Coupons.FirstOrDefault(c => c.Code == CouponCode);

                if (coupon == null)
                {
                    TempData["Error"] = "Mã không tồn tại";
                    return RedirectToAction("Checkout");
                }

                if (!coupon.IsActive)
                {
                    TempData["Error"] = "Mã đã bị khóa";
                    return RedirectToAction("Checkout");
                }

                if (coupon.ExpiryDate < DateTime.Now)
                {
                    TempData["Error"] = "Mã đã hết hạn";
                    return RedirectToAction("Checkout");
                }

                // 🔥 CHECK: USER ĐÃ DÙNG CHƯA
                var used = _context.UserCoupons
                    .Any(x => x.UserEmail == email && x.CouponId == coupon.Id);

                if (used)
                {
                    TempData["Error"] = "Bạn đã sử dụng mã này rồi";
                    return RedirectToAction("Checkout");
                }

                if (coupon.Discount.HasValue)
                {
                    total = total - (total * coupon.Discount.Value / 100);

                    order.CouponCode = coupon.Code;
                    order.DiscountPercent = coupon.Discount;
                }
            }

            if (total < 0) total = 0;

            order.TotalPrice = total;

            // =========================

            order.OrderDetails = new List<OrderDetail>();

            foreach (var item in cartItems)
            {
                if (item.Product.Quantity >= item.Quantity)
                {
                    item.Product.Quantity -= item.Quantity;
                }
                else
                {
                    TempData["Error"] = $"Sản phẩm {item.Product.Name} không đủ hàng.";
                    return RedirectToAction("Checkout");
                }

                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price ?? 0
                });
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            // =========================
            // 🔥 LƯU LỊCH SỬ DÙNG MÃ
            // =========================
            if (coupon != null)
            {
                _context.UserCoupons.Add(new UserCoupon
                {
                    UserEmail = email,
                    CouponId = coupon.Id,
                    UsedDate = DateTime.Now
                });

                _context.SaveChanges();
            }

            // =========================

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

        //Trang danh sách đơn hàng của người dùng
        public IActionResult Index()
        {
            var email = HttpContext.Session.GetString("UserEmail");

            if (email == null)
                return RedirectToAction("Login", "Auth");

            var orders = _context.Orders
                .Where(o => o.UserId == email)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        //Xem chi tiết đơn hàng
        public IActionResult Detail(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }
    }
}