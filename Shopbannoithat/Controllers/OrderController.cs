using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shopbannoithat.Data;
using Shopbannoithat.Models;
using Shopbannoithat.Services;
using Microsoft.Extensions.DependencyInjection;

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
        .ThenInclude(p => p.Variants)
            .ThenInclude(v => v.Size)
    .Include(x => x.Product)
        .ThenInclude(p => p.Variants)
            .ThenInclude(v => v.Material)
    .Include(x => x.Product)
        .ThenInclude(p => p.Variants)
            .ThenInclude(v => v.Color)
    .Where(x => x.UserEmail == email)
    .ToList();

            return View(cartItems);
        }

        // Xác nhận đặt hàng
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(Order order, string CouponCode)
        {

            var email = HttpContext.Session.GetString("UserEmail");

            var cartItems = _context.Carts
                .Include(x => x.Product)
                .ThenInclude(p => p.Variants)
                .Where(x => x.UserEmail == email)
                .ToList();

            // VALIDATE
            if (string.IsNullOrWhiteSpace(order.Name) ||
                string.IsNullOrWhiteSpace(order.Address1) ||
                string.IsNullOrWhiteSpace(order.Address2) ||
                string.IsNullOrWhiteSpace(order.City))
            {
                if (string.IsNullOrWhiteSpace(order.Name))
                    ModelState.AddModelError("Name", "Vui lòng nhập tên");

                if (string.IsNullOrWhiteSpace(order.Address1))
                    ModelState.AddModelError("Address1", "Vui lòng nhập tổ ấp");

                if (string.IsNullOrWhiteSpace(order.Address2))
                    ModelState.AddModelError("Address2", "Vui lòng nhập xã phường");

                if (string.IsNullOrWhiteSpace(order.City))
                    ModelState.AddModelError("City", "Vui lòng nhập thành phố");

                return View("Checkout", cartItems);
            }

            if (!cartItems.Any())
                return RedirectToAction("Checkout");

            order.UserId = email;
            order.OrderDate = DateTime.Now;
            order.Status = "Đang xử lý";
            order.OrderPlaced = DateTime.Now;
            order.PaymentMethod = order.PaymentMethod ?? "COD"; // 🔥 Thêm
            order.IsPaid = order.PaymentMethod == "COD";        // 🔥 COD thì coi như đã xử lý

            decimal total = 0;
            order.OrderDetails = new List<OrderDetail>();

            foreach (var item in cartItems)
            {
                // Trước - tìm variant
                var variant = _context.ProductVariants.FirstOrDefault(v =>
                    v.ProductId == item.ProductId &&
                    v.SizeId == item.SizeId &&
                    v.MaterialId == item.MaterialId &&
                    v.ColorId == item.ColorId
                );

                // 🔥 THÊM: bắt buộc phải có variant
                if (variant == null)
                {
                    TempData["Error"] = $"Sản phẩm '{item.Product.Name}' không tìm thấy biến thể phù hợp!";
                    return RedirectToAction("Checkout");
                }

                decimal price = variant.Price; // ✅ không cần ?? nữa vì chắc chắn có variant
                int stock = variant.Stock;

                // CHECK TỒN KHO
                if (stock < item.Quantity)
                {
                    TempData["Error"] = $"Sản phẩm {item.Product.Name} không đủ hàng.";
                    return RedirectToAction("Checkout");
                }

                // TRỪ KHO
                variant.Stock -= item.Quantity;
                item.Product.Quantity -= item.Quantity;

                total += price * item.Quantity;

                // LƯU CHI TIẾT ĐƠN
                order.OrderDetails.Add(new OrderDetail
                {
                    ProductId = item.ProductId,
                    VariantId = variant.Id,  // ✅ không còn ?. vì chắc chắn có variant
                    Quantity = item.Quantity,
                    Price = price,
                    SizeId = item.SizeId,
                    MaterialId = item.MaterialId,
                    ColorId = item.ColorId
                });
            }

            // ===== COUPON =====
            if (!string.IsNullOrEmpty(CouponCode))
            {
                var coupon = _context.Coupons.FirstOrDefault(c => c.Code == CouponCode);

                if (coupon == null)
                {
                    TempData["Error"] = "Mã không tồn tại";
                    return RedirectToAction("Checkout");
                }

                if (!coupon.IsActive || coupon.ExpiryDate < DateTime.Now)
                {
                    TempData["Error"] = "Mã không hợp lệ";
                    return RedirectToAction("Checkout");
                }

                var used = _context.UserCoupons
                    .Any(x => x.UserEmail == email && x.CouponId == coupon.Id);

                if (used)
                {
                    TempData["Error"] = "Bạn đã dùng mã này rồi";
                    return RedirectToAction("Checkout");
                }

                if (coupon.Discount.HasValue)
                {
                    total -= total * coupon.Discount.Value / 100;

                    order.CouponCode = coupon.Code;
                    order.DiscountPercent = coupon.Discount;

                    _context.UserCoupons.Add(new UserCoupon
                    {
                        UserEmail = email,
                        CouponId = coupon.Id,
                        UsedDate = DateTime.Now
                    });
                }
            }

            if (total < 0) total = 0;
            order.TotalPrice = total;

            _context.Orders.Add(order);

            // XÓA GIỎ
            _context.Carts.RemoveRange(cartItems);

            _context.SaveChanges();
            // ================== GỬI HÓA ĐƠN QUA EMAIL (chỉ cho user Google) ==================
            if (order.IsPaid || order.PaymentMethod == "BANK")   // Bạn có thể điều chỉnh điều kiện này
            {
                try
                {
                    // Hiện tại project của bạn dùng Session + UserEmail, chưa có cột Provider
                    // Tạm thời gửi cho tất cả (sau này bạn thêm cột Provider vào bảng User thì mình chỉnh lại)
                    var emailService = HttpContext.RequestServices.GetRequiredService<IEmailService>();
                    await emailService.SendOrderInvoiceAsync(order);
                }
                catch (Exception ex)
                {
                    // Không để lỗi email làm hỏng quá trình đặt hàng
                    Console.WriteLine($"Gửi email thất bại: {ex.Message}");
                }
            }

            return RedirectToAction("Success", new { id = order.Id });
        }

        // Trang đặt hàng thành công
        public IActionResult Success(int id)
        {
            var order = _context.Orders
        .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
        .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Variant)
                .ThenInclude(v => v.Size)
        .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Variant)
                .ThenInclude(v => v.Material)
        .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Variant)
                .ThenInclude(v => v.Color)
        .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();

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

        // Product
        .Include(o => o.OrderDetails)
            .ThenInclude(d => d.Product)

        // Variant + Size
        .Include(o => o.OrderDetails)
            .ThenInclude(d => d.Variant)
                .ThenInclude(v => v.Size)

        // Variant + Material
        .Include(o => o.OrderDetails)
            .ThenInclude(d => d.Variant)
                .ThenInclude(v => v.Material)

        // Variant + Color
        .Include(o => o.OrderDetails)
            .ThenInclude(d => d.Variant)
                .ThenInclude(v => v.Color)

        .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }
    }
}