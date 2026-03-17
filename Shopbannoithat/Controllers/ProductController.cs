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

            // Tìm ID của danh mục cha (Phòng khách...)
            var parentCategory = _context.Categories
                .FirstOrDefault(c => c.Name == categoryName && c.ParentId == null);

            if (parentCategory == null)
                return NotFound();

            // Lấy tất cả ID danh mục con thuộc phòng này
            var childIds = _context.Categories
                .Where(c => c.ParentId == parentCategory.Id)
                .Select(c => c.Id)
                .ToList();

            // Thêm cả ID cha vào để lọc
            childIds.Add(parentCategory.Id);

            // Lấy sản phẩm thuộc bất kỳ danh mục nào trong nhóm này
            var query = _context.Products
                .Include(p => p.Category)
                .ThenInclude(c => c.Parent)
                .Where(p => p.CategoryId != null && childIds.Contains(p.CategoryId.Value))
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

        // API lấy danh mục con theo phòng (cho mega menu)
        public IActionResult GetSubCategories()
        {
            var categories = _context.Categories
                .Where(c => c.ParentId != null)
                .Select(c => new {
                    c.Id,
                    c.Name,
                    c.ParentId,
                    ParentName = c.Parent.Name
                })
                .ToList();

            return Json(categories);
        }

        public IActionResult BySubCategory(int categoryId, string sort)
        {
            var category = _context.Categories
                .Include(c => c.Parent)
                .FirstOrDefault(c => c.Id == categoryId);

            if (category == null) return NotFound();

            var query = _context.Products
                .Include(p => p.Category)
                .ThenInclude(c => c.Parent)
                .Where(p => p.CategoryId == categoryId)
                .AsQueryable();

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            ViewBag.CategoryName = category.Name;
            ViewBag.ParentName = category.Parent?.Name;
            ViewBag.CategoryId = categoryId;
            ViewBag.CurrentSort = sort;

            return View("ByCategory", query.ToList());
        }
    }
}