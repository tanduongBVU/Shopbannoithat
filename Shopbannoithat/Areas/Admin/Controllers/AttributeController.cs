using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")] /*Controller này thuộc khu vực Admin*/
    public class AttributeController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AttributeController(ApplicationDbContext context) => _context = context; /*Nhận kết nối database qua Dependency Injection*/

        public IActionResult Index() /*Hiển thị danh sách các thuộc tính*/
        {
            ViewBag.Sizes = _context.Sizes.ToList();
            ViewBag.Materials = _context.Materials.ToList();
            ViewBag.Colors = _context.Colors.ToList();
            return View();
        }
    }
}
