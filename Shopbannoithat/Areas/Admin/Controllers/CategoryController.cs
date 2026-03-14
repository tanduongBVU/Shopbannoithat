using Microsoft.AspNetCore.Mvc;
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
            var categories = _context.Categories.ToList();
            return View(categories);
        }

        // GET Create
        public IActionResult Create()
        {
            return View();
        }

        // POST Create
        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            _context.Categories.Add(category);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var category = _context.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {

            if (!ModelState.IsValid)
            {
                return View(category);
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
