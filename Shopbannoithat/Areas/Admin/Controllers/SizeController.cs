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
            _context.Sizes.Add(size);
            _context.SaveChanges();
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
