using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SizeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SizeController(ApplicationDbContext context) => _context = context;

        [HttpPost]
        public IActionResult Create(ProductSize size)
        {
            // Kiểm tra tên đã tồn tại chưa (không phân biệt hoa thường)
            if (_context.Sizes.Any(s => s.Name.ToLower() == size.Name.ToLower().Trim()))
            {
                TempData["SizeError"] = $"Kích thước '{size.Name}' đã tồn tại!";
                return RedirectToAction("Index", "Attribute");
            }

            size.Name = size.Name.Trim();
            _context.Sizes.Add(size);
            _context.SaveChanges();
            TempData["SizeSuccess"] = "Thêm kích thước thành công!";
            return RedirectToAction("Index", "Attribute");
        }

        public IActionResult Delete(int id)
        {
            var size = _context.Sizes.Find(id);
            if (size != null) { _context.Sizes.Remove(size); _context.SaveChanges(); }
            return RedirectToAction("Index", "Attribute");
        }
    }
}
