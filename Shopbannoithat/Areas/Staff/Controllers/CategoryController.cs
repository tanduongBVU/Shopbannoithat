using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
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
            var categories = _context.Categories
                .Include(c => c.Parent)
                .Include(c => c.Children)
                .ToList();
            return View(categories);
        }

        public IActionResult Create()
        {
            // Chỉ lấy danh mục cha (Phòng) để chọn
            ViewBag.Parents = _context.Categories
                .Where(c => c.ParentId == null)
                .ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Parents = _context.Categories
                    .Where(c => c.ParentId == null)
                    .ToList();
                return View(category);
            }

            // Kiểm tra tên danh mục đã tồn tại chưa (cùng cấp)
            bool isDuplicate = _context.Categories.Any(c =>
                c.Name.ToLower() == category.Name.ToLower().Trim() &&
                c.ParentId == category.ParentId // cùng cấp: cùng cha hoặc cùng là cấp 1
            );

            if (isDuplicate)
            {
                // Gắn lỗi vào form thay vì TempData
                ModelState.AddModelError("Name",
                    category.ParentId == null
                    ? $"Phòng '{category.Name}' đã tồn tại!"
                    : $"Danh mục '{category.Name}' đã tồn tại trong phòng này!");

                ViewBag.Parents = _context.Categories
                    .Where(c => c.ParentId == null)
                    .ToList();
                return View(category);
            }

            category.Name = category.Name.Trim();
            _context.Categories.Add(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            ViewBag.Parents = _context.Categories
                .Where(c => c.ParentId == null && c.Id != id)
                .ToList();
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Parents = _context.Categories
                    .Where(c => c.ParentId == null)
                    .ToList();
                return View(category);
            }

            // Lấy ParentId cũ từ database, không cho form ghi đè
            var existing = _context.Categories
                .AsNoTracking()
                .FirstOrDefault(c => c.Id == category.Id);

            if (existing != null)
            {
                category.ParentId = existing.ParentId;
            }

            _context.Categories.Update(category);
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var category = _context.Categories.Find(id);

            if (category == null)
            {
                return NotFound();
            }

            // Kiểm tra xem danh mục này có danh mục con không
            bool hasChildren = _context.Categories.Any(c => c.ParentId == id);

            if (hasChildren)
            {
                return RedirectToAction("Index");
            }

            // Nếu không có con → cho phép xóa
            _context.Categories.Remove(category);
            _context.SaveChanges();

            TempData["Success"] = "Xóa danh mục thành công.";
            return RedirectToAction("Index");
        }
    }
}
