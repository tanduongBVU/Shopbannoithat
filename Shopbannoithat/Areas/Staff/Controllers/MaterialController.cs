using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class MaterialController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MaterialController(ApplicationDbContext context) => _context = context;

        [HttpPost]
        public IActionResult Create(ProductMaterial material)
        {
            // Kiểm tra tên đã tồn tại chưa
            if (_context.Materials.Any(m => m.Name.ToLower() == material.Name.ToLower().Trim()))
            {
                TempData["MaterialError"] = $"Vật liệu '{material.Name}' đã tồn tại!";
                return RedirectToAction("Index", "Attribute");
            }

            material.Name = material.Name.Trim();
            _context.Materials.Add(material);
            _context.SaveChanges();
            TempData["MaterialSuccess"] = "Thêm vật liệu thành công!";
            return RedirectToAction("Index", "Attribute");
        }

        public IActionResult Delete(int id)
        {
            var material = _context.Materials.Find(id);
            if (material != null) { _context.Materials.Remove(material); _context.SaveChanges(); }
            return RedirectToAction("Index", "Attribute");
        }
    }
}
