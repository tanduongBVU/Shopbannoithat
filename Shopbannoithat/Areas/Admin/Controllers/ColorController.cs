using Microsoft.AspNetCore.Mvc;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ColorController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ColorController(ApplicationDbContext context) => _context = context;

        [HttpPost]
        public IActionResult Create(ProductColor color)  /*Thêm màu*/
        {
            _context.Colors.Add(color); /*Thêm object "color" vào bảng Colors trong database*/
            _context.SaveChanges();
            return RedirectToAction("Index", "Attribute");  /*chuyển sang action index của controller Attribute*/
        }

        public IActionResult Delete(int id)   /*Xóa màu*/
        {
            var color = _context.Colors.Find(id);  
            if (color != null) { _context.Colors.Remove(color); _context.SaveChanges(); }  /*nếu color tồn tại thì xóa color đó ra khỏi database*/
            return RedirectToAction("Index", "Attribute");  
        }
    }
}
