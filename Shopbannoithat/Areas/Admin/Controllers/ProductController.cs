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
            var role = HttpContext.Session.GetString("UserRole");

            // chưa đăng nhập hoặc không phải Staff
            if (string.IsNullOrEmpty(role) || role != "Admin")
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }
            var products = _context.Products
        .Include(p => p.Category)
        .ThenInclude(c => c.Parent)
        .Include(p => p.Variants) // 🔥 Thêm
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
            // 🔥 Thêm 3 dòng này
            ViewBag.Sizes = _context.Sizes.ToList();
            ViewBag.Materials = _context.Materials.ToList();
            ViewBag.Colors = _context.Colors.ToList();
            
            return View();
        }

        [HttpPost]
        public IActionResult Create(Product product, IFormFile imageFile, List<ProductVariant> Variants)
        {

            // 🔥 THÊM: Bắt buộc phải có ít nhất 1 biến thể
            if (Variants == null || !Variants.Any())
            {
                ModelState.AddModelError("", "Vui lòng thêm ít nhất một biến thể sản phẩm!");

                ViewBag.Parents = _context.Categories.Where(c => c.ParentId == null).ToList();
                var ch = _context.Categories.Where(c => c.ParentId != null)
                                 .Select(c => new { c.Id, c.Name, c.ParentId }).ToList();
                ViewBag.ChildrenJson = System.Text.Json.JsonSerializer.Serialize(ch,
                    new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
                ViewBag.Sizes = _context.Sizes.ToList();
                ViewBag.Materials = _context.Materials.ToList();
                ViewBag.Colors = _context.Colors.ToList();
                return View(product);
            }

            // 🔹 Tự sinh SKU nếu trống
            if (string.IsNullOrEmpty(product.SKU))
            {
                // Lấy ID lớn nhất hiện có
                var maxId = _context.Products.Any() ? _context.Products.Max(p => p.Id) + 1 : 1;
                product.SKU = "AD" + maxId.ToString("D4"); // AD0001, AD0002 ...
            }
            else
            {
                // Kiểm tra trùng lặp
                if (_context.Products.Any(p => p.SKU == product.SKU))
                {
                    ModelState.AddModelError("SKU", "Mã sản phẩm (SKU) đã tồn tại!");
                }
            }
            if (imageFile == null)
                ModelState.AddModelError("Image", "Vui lòng chọn hình ảnh");

            if (!ModelState.IsValid)
            {
                ViewBag.Parents = _context.Categories.Where(c => c.ParentId == null).ToList();
                var ch = _context.Categories.Where(c => c.ParentId != null)
                                 .Select(c => new { c.Id, c.Name, c.ParentId }).ToList();
                ViewBag.ChildrenJson = System.Text.Json.JsonSerializer.Serialize(ch,
                    new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
                ViewBag.Sizes = _context.Sizes.ToList();
                ViewBag.Materials = _context.Materials.ToList();
                ViewBag.Colors = _context.Colors.ToList();
                return View(product);
            }

            string fileName = Path.GetFileName(imageFile.FileName);
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
            using (var stream = new FileStream(path, FileMode.Create))
                imageFile.CopyTo(stream);

            product.Image = fileName;
            product.Price = Variants?.Min(v => v.Price) ?? 0; // 🔥 Thêm vào đây
            product.Variants = null; // 🔥 Quan trọng: tránh EF tự track variants


            // 🔥 Kiểm tra variant có đầy đủ thông tin không
            if (Variants != null && Variants.Any())
            {
                foreach (var v in Variants)
                {
                    if (v.SizeId == null || v.MaterialId == null || v.ColorId == null || v.Price <= 0)
                    {
                        ModelState.AddModelError("", "Vui lòng chọn đầy đủ kích thước, vật liệu, màu sắc và giá cho tất cả biến thể!");

                        ViewBag.Parents = _context.Categories.Where(c => c.ParentId == null).ToList();
                        var ch = _context.Categories.Where(c => c.ParentId != null)
                                         .Select(c => new { c.Id, c.Name, c.ParentId }).ToList();
                        ViewBag.ChildrenJson = System.Text.Json.JsonSerializer.Serialize(ch,
                            new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });
                        ViewBag.Sizes = _context.Sizes.ToList();
                        ViewBag.Materials = _context.Materials.ToList();
                        ViewBag.Colors = _context.Colors.ToList();

                        return View(product);
                    }
                }
            }
            // 🔥 CHECK VARIANT TRƯỚC KHI LƯU PRODUCT
            if (Variants != null && Variants.Any())
            {
                var totalStock = Variants.Sum(v => v.Stock);

                if (totalStock > product.Quantity)
                {
                    ModelState.AddModelError("", "Tổng tồn kho biến thể không được vượt quá số lượng sản phẩm gốc!");

                    // load lại ViewBag
                    ViewBag.Parents = _context.Categories.Where(c => c.ParentId == null).ToList();

                    var ch = _context.Categories.Where(c => c.ParentId != null)
                        .Select(c => new { c.Id, c.Name, c.ParentId }).ToList();

                    ViewBag.ChildrenJson = System.Text.Json.JsonSerializer.Serialize(ch,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                        });

                    ViewBag.Sizes = _context.Sizes.ToList();
                    ViewBag.Materials = _context.Materials.ToList();
                    ViewBag.Colors = _context.Colors.ToList();

                    return View(product);
                }
            }

            _context.Products.Add(product);
            _context.SaveChanges();



            // 🔥 Lưu variants riêng biệt
            if (Variants != null && Variants.Any())
            {
                foreach (var v in Variants)
                {
                    v.ProductId = product.Id;
                    _context.ProductVariants.Add(v);
                }
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public IActionResult Edit(int id)
        {
            var product = _context.Products
    .Include(p => p.Variants)
    .ThenInclude(v => v.Size)
    .Include(p => p.Variants)
    .ThenInclude(v => v.Material)
    .Include(p => p.Variants)
    .ThenInclude(v => v.Color)
    .AsNoTracking() // 🔥 THÊM DÒNG NÀY
    .FirstOrDefault(p => p.Id == id);

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
            ViewBag.Sizes = _context.Sizes.ToList();
            ViewBag.Materials = _context.Materials.ToList();
            ViewBag.Colors = _context.Colors.ToList();

            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product, IFormFile imageFile, List<ProductVariant> Variants)
        {
            var existingProduct = _context.Products
         .Include(p => p.Variants)
         .FirstOrDefault(p => p.Id == product.Id);

            if (existingProduct == null)
                return NotFound();

            // VALIDATE MODEL
            if (!ModelState.IsValid)
            {
                product.Variants = existingProduct.Variants;
                LoadViewBag(product.Id);
                return View(product);
            }

            // UPDATE PRODUCT
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Quantity = product.Quantity;
            existingProduct.CategoryId = product.CategoryId;

            // ẢNH
            if (imageFile != null)
            {
                string fileName = Path.GetFileName(imageFile.FileName);
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    imageFile.CopyTo(stream);
                }

                existingProduct.Image = fileName;
            }

            // VALIDATE VARIANTS
            if (Variants != null && Variants.Any())
            {
                foreach (var v in Variants)
                {
                    if (v.SizeId == null || v.SizeId == 0 ||
                        v.MaterialId == null || v.MaterialId == 0 ||
                        v.ColorId == null || v.ColorId == 0)
                    {
                        ModelState.AddModelError("", "Thiếu thông tin biến thể!");

                        product.Variants = existingProduct.Variants;
                        LoadViewBag(product.Id);
                        return View(product);
                    }
                }

                var totalStock = Variants.Sum(v => v.Stock);
                if (totalStock > product.Quantity)
                {
                    ModelState.AddModelError("", "Tồn kho vượt quá!");

                    product.Variants = existingProduct.Variants;
                    LoadViewBag(product.Id);
                    return View(product);
                }
            }

            // UPDATE GIÁ
            existingProduct.Price = (Variants != null && Variants.Any())
                ? Variants.Min(v => v.Price)
                : 0;

            _context.SaveChanges(); // lưu product

            // =====================================================
            // 🔥 XỬ LÝ VARIANTS ĐÚNG CÁCH
            // =====================================================

            var oldVariants = existingProduct.Variants.ToList();

            // ❗ B1: Null VariantId trong OrderDetails trước khi xóa
            var variantIdsToDelete = oldVariants
                .Where(old => Variants == null || !Variants.Any(v =>
                    v.SizeId == old.SizeId &&
                    v.MaterialId == old.MaterialId &&
                    v.ColorId == old.ColorId))
                .Select(v => v.Id)
                .ToList();

            if (variantIdsToDelete.Any())
            {
                // Xóa luôn OrderDetails có variant bị xóa (vì VariantId NOT NULL)
                var affectedOrders = _context.OrderDetails
                    .Where(od => variantIdsToDelete.Contains(od.VariantId))
                    .ToList();

                _context.OrderDetails.RemoveRange(affectedOrders); // ✅ xóa thay vì set null
                _context.SaveChanges();

                // Xóa cart items liên quan
                var affectedCarts = _context.Carts
                    .Where(c => c.ProductId == product.Id)
                    .ToList();
                _context.Carts.RemoveRange(affectedCarts);
                _context.SaveChanges();
            }

            // ❗ B2: XÓA những variant KHÔNG còn tồn tại trên form
            foreach (var old in oldVariants)
            {
                var stillExist = Variants != null && Variants.Any(v =>
                    v.SizeId == old.SizeId &&
                    v.MaterialId == old.MaterialId &&
                    v.ColorId == old.ColorId);

                if (!stillExist)
                    _context.ProductVariants.Remove(old);
            }
            _context.SaveChanges();

            // ❗ B3: UPDATE hoặc ADD
            if (Variants != null)
            {
                foreach (var v in Variants)
                {
                    var existing = oldVariants.FirstOrDefault(x =>
                        x.SizeId == v.SizeId &&
                        x.MaterialId == v.MaterialId &&
                        x.ColorId == v.ColorId);

                    if (existing != null)
                    {
                        existing.Price = v.Price;
                        existing.Stock = v.Stock;
                    }
                    else
                    {
                        v.ProductId = product.Id;
                        _context.ProductVariants.Add(v);
                    }
                }
            }
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        // 🔥 Helper tránh lặp code ViewBag
        private void LoadViewBag(int productId = 0)
        {
            ViewBag.Parents = _context.Categories.Where(c => c.ParentId == null).ToList();

            var ch = _context.Categories
                .Where(c => c.ParentId != null)
                .Select(c => new { c.Id, c.Name, c.ParentId }).ToList();

            ViewBag.ChildrenJson = System.Text.Json.JsonSerializer.Serialize(ch,
                new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Sizes = _context.Sizes.ToList();
            ViewBag.Materials = _context.Materials.ToList();
            ViewBag.Colors = _context.Colors.ToList();
        }
        public IActionResult Delete(int id)
        {
            var product = _context.Products.Find(id);

            _context.Products.Remove(product);

            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        public IActionResult Detail(int id)
        {
            var product = _context.Products
        .Include(p => p.Category)
        .ThenInclude(c => c.Parent)
        .Include(p => p.Variants)
        .ThenInclude(v => v.Size)
        .Include(p => p.Variants)
        .ThenInclude(v => v.Material)
        .Include(p => p.Variants)
        .ThenInclude(v => v.Color)
        .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }


    }
}
