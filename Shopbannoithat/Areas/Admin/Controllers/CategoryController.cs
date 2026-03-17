using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
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
            if (category != null)
            {
                _context.Categories.Remove(category);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}