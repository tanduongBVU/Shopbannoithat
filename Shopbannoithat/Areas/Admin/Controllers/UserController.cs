using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Danh sách user
        public IActionResult Index()
        {
            var users = _context.Users.ToList();
            return View(users);
        }

        // Trang phân quyền
        public IActionResult ChangeRole(int id)
        {
            var user = _context.Users.Find(id);

            // lấy danh sách role từ database
            ViewBag.Roles = _context.Roles.ToList();

            return View(user);
        }

        // Lưu role
        [HttpPost]
        public IActionResult ChangeRole(int id, string role)
        {
            var user = _context.Users.Find(id);

            if (user != null)
            {
                user.Role = role;
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var user = _context.Users.Find(id);

            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }
    }
}
