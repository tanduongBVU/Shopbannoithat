using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CouponController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
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
