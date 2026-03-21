using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AttributeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AttributeController(ApplicationDbContext context) => _context = context;

        public IActionResult Index()
        {
            ViewBag.Sizes = _context.Sizes.ToList();
            ViewBag.Materials = _context.Materials.ToList();
            ViewBag.Colors = _context.Colors.ToList();
            return View();
        }
    }
}
