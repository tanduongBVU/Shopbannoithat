using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbannoithat.Data;

namespace Shopbannoithat.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(string sort)
        {
            var products = _context.Products.AsQueryable();

            switch (sort)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                    products = products.OrderByDescending(p => p.CreatedAt);
                    break;
                default:
                    products = products.OrderByDescending(p => p.Id);
                    break;
            }

            ViewBag.CurrentSort = sort;
            return View(products.ToList());
        }

        public async Task<IActionResult> Details(int id)
        {
            if (id == 0) return NotFound();

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            return View(product);
        }

        public IActionResult Search(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                return RedirectToAction("Index");
            }

            keyword = keyword.ToLower();

            var products = _context.Products
    .Where(p => p.Name.Contains(keyword) ||
                p.Description.Contains(keyword) ||
                p.Category.Name.Contains(keyword))
    .ToList();

            return View("Index", products);
        }

        public JsonResult SearchAjax(string keyword)
        {
            var data = _context.Products
                .Where(p => p.Name.Contains(keyword))
                .Select(p => new
                {
                    id = p.Id,
                    name = p.Name
                })
                .Take(5)
                .ToList();

            return Json(data);
        }

        public IActionResult ByCategory(string slug, string sort)
        {
            // Map slug → tên danh mục
            var categoryMap = new Dictionary<string, string>
        {
            { "phong-khach", "Phòng khách" },
            { "phong-ngu", "Phòng ngủ" },
            { "phong-bep", "Phòng bếp" },
            { "phong-lam-viec", "Phòng làm việc" }
        };

            if (!categoryMap.ContainsKey(slug))
                return NotFound();

            var categoryName = categoryMap[slug];

            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Category != null && p.Category.Name == categoryName)
                .AsQueryable();

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            ViewBag.CategoryName = categoryName;
            ViewBag.Slug = slug;
            ViewBag.CurrentSort = sort;

            return View(query.ToList());
        }
    }
}