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
            _context.Materials.Add(material);
            _context.SaveChanges();
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
