using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class RoleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RoleController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");

            // chưa đăng nhập hoặc không phải Staff
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            var roles = _context.Roles.ToList();
            return View(roles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Role role)
        {
            if (!ModelState.IsValid)
            {
                return View(role);
            }
            _context.Roles.Add(role);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var role = _context.Roles.Find(id);
            return View(role);
        }

        [HttpPost]
        public IActionResult Edit(Role role)
        {
            if (!ModelState.IsValid)
            {
                return View(role);
            }
            _context.Roles.Update(role);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        public IActionResult Delete(int id)
        {
            var role = _context.Roles.Find(id);

            _context.Roles.Remove(role);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
    }
}
