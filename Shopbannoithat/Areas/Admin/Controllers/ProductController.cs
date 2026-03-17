using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products
                .Include(p => p.Category)
                .ThenInclude(c => c.Parent)
                .ToList();

            return View(products);
        }

        public IActionResult Create()
        {
            // Danh mục cha (Phòng)
            ViewBag.Parents = _context.Categories
                .Where(c => c.ParentId == null)
                .ToList();

            // Danh mục con dạng JSON cho JS
            var children = _context.Categories
                .Where(c => c.ParentId != null)
                .Select(c => new { c.Id, c.Name, c.ParentId })
                .ToList();

            ViewBag.ChildrenJson = System.Text.Json.JsonSerializer.Serialize(children,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product, IFormFile imageFile)
        {
            if (imageFile == null)
            {
                ModelState.AddModelError("Image", "Vui lòng chọn hình ảnh");
            }

            if (!ModelState.IsValid)
            {
                // ❗ LOAD LẠI Y NHƯ GET
                ViewBag.Parents = _context.Categories
                    .Where(c => c.ParentId == null)
                    .ToList();

                var children = _context.Categories
                    .Where(c => c.ParentId != null)
                    .Select(c => new { c.Id, c.Name, c.ParentId })
                    .ToList();

                ViewBag.ChildrenJson = System.Text.Json.JsonSerializer.Serialize(children,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    });

                return View(product);
            }

            // upload file
            string fileName = Path.GetFileName(imageFile.FileName);

            string path = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/images",
                fileName
            );

            using (var stream = new FileStream(path, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            product.Image = fileName;

            _context.Products.Add(product);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var product = _context.Products.Find(id);

            ViewBag.Parents = _context.Categories
                .Where(c => c.ParentId == null)
                .ToList();

            var children = _context.Categories
                .Where(c => c.ParentId != null)
                .Select(c => new { c.Id, c.Name, c.ParentId })
                .ToList();

            ViewBag.ChildrenJson = System.Text.Json.JsonSerializer.Serialize(children,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

            ViewBag.Categories = _context.Categories.ToList();

            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product, IFormFile imageFile)
        {
            if (imageFile == null)
            {
                ModelState.AddModelError("Image", "Vui lòng chọn hình ảnh");
            }

            if (!ModelState.IsValid)
            {
                // load lại ViewBag (QUAN TRỌNG)
                ViewBag.Parents = _context.Categories
                    .Where(c => c.ParentId == null)
                    .ToList();

                var children = _context.Categories
                    .Where(c => c.ParentId != null)
                    .Select(c => new { c.Id, c.Name, c.ParentId })
                    .ToList();

                ViewBag.ChildrenJson = System.Text.Json.JsonSerializer.Serialize(children,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                    });

                ViewBag.Categories = _context.Categories.ToList();

                return View(product);
            }

            // upload file
            string fileName = Path.GetFileName(imageFile.FileName);
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                imageFile.CopyTo(stream);
            }

            product.Image = fileName;

            _context.Products.Update(product);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);

            _context.Products.Remove(product);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }


    }
}
