using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class ColorController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ColorController(ApplicationDbContext context) => _context = context;

        [HttpPost]
        public IActionResult Create(ProductColor color)
        {
            // Kiểm tra tên đã tồn tại
            if (_context.Colors.Any(c => c.Name.ToLower() == color.Name.ToLower().Trim()))
            {
                TempData["ColorError"] = $"Màu '{color.Name}' đã tồn tại!";
                return RedirectToAction("Index", "Attribute");
            }

            // Kiểm tra mã HEX đã tồn tại
            if (_context.Colors.Any(c => c.HexCode.ToLower() == color.HexCode.ToLower().Trim()))
            {
                TempData["ColorError"] = $"Mã màu '{color.HexCode}' đã tồn tại!";
                return RedirectToAction("Index", "Attribute");
            }

            color.Name = color.Name.Trim();
            color.HexCode = color.HexCode.Trim();
            _context.Colors.Add(color);
            _context.SaveChanges();
            TempData["ColorSuccess"] = "Thêm màu thành công!";
            return RedirectToAction("Index", "Attribute");
        }

        public IActionResult Delete(int id)
        {
            var color = _context.Colors.Find(id);
            if (color != null) { _context.Colors.Remove(color); _context.SaveChanges(); }
            return RedirectToAction("Index", "Attribute");
        }
    }
}
