using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            // chưa đăng nhập hoặc không phải Staff
            if (string.IsNullOrEmpty(role) || role != "Staff")
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            var coupons = _context.Coupons.ToList();
            return View(coupons);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Coupon coupon)
        {
            if (!ModelState.IsValid)
            {
                return View(coupon);
            }
            _context.Coupons.Add(coupon);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var coupon = _context.Coupons.Find(id);

            if (coupon != null)
            {
                _context.Coupons.Remove(coupon);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Toggle(int id)
        {
            var coupon = _context.Coupons.Find(id);

            if (coupon != null)
            {
                coupon.IsActive = !coupon.IsActive;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
