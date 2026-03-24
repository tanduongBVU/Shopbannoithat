using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Shopbannoithat.Data;
using Shopbannoithat.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]  /*Controller này thuộc khu vực Admin*/
    public class CategoryController : Controller
    {
        //kết nối database vào Controller.
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index() /*Hiển thị danh sách các danh mục, bao gồm cả danh mục cha và con*/
        {
            var categories = _context.Categories /*truy cập bảng categoy trong database*/
                .Include(c => c.Parent)  /*Lấy kèm danh mục cha*/
                .Include(c => c.Children) /*Lấy kèm danh mục con*/
                .ToList(); /*Thực thi câu truy vấn, chuyển kết quả thành danh sác*/
            return View(categories);  /*hiển thị lên view*/
        }

        public IActionResult Create() /*Hiển thị form tạo mới danh mục*/ 
        {
            // Chỉ lấy danh mục cha (Phòng) để chọn
            ViewBag.Parents = _context.Categories 
                .Where(c => c.ParentId == null) /*nếu tạo danh mục mà không chọn danh mục thì danh mục đó sẽ là danh mục cha*/
                .ToList();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category) /*Xử lý dữ liệu từ form tạo mới danh mục, kiểm tra hợp lệ và lưu vào database*/
        {
            if (!ModelState.IsValid)  /*bắt lỗi( bỏ trống,sai định dạng)*/
            {
                ViewBag.Parents = _context.Categories
                    .Where(c => c.ParentId == null)
                    .ToList();
                return View(category);
            }
            _context.Categories.Add(category);  /*thêm tt danh mục mói và tạm chưa thêm vào database*/
            _context.SaveChanges();  /*thực thi câu lệnh và lưu vào database*/
            return RedirectToAction("Index");  /*chuyển sang trang index*/
        }

        public IActionResult Edit(int id) /*Hiển thị form chỉnh sửa danh mục, lấy dữ liệu của danh mục cần chỉnh sửa và danh sách danh mục cha để chọn lại nếu cần*/
        {
            var category = _context.Categories.Find(id);  /*tìm danh mục có id trong bảng catefory*/
            ViewBag.Parents = _context.Categories
                .Where(c => c.ParentId == null && c.Id != id)
                .ToList();
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category) /*Xử lý dữ liệu từ form chỉnh sửa danh mục, ktra hợp lệ và cập nhật vào database*/
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Parents = _context.Categories
                    .Where(c => c.ParentId == null)
                    .ToList();
                return View(category);
            }

            // Lấy ParentId cũ từ database, không cho form ghi đè
            var existing = _context.Categories
                .AsNoTracking()  /*lấy dữ liệu (ParentId) khi không cần sửa*/
                .FirstOrDefault(c => c.Id == category.Id); /*là tìm danh mục có id trung với id trong bảng category*/

            if (existing != null) /*Nếu tìm thấy danh mục trong database*/
            {
                category.ParentId = existing.ParentId; /*Lấy ParentId từ database gán vào category*/
            }

            _context.Categories.Update(category); /*Báo cho EF Core biết object "category" đã thay đổi cần UPDATE vào database*/

            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id) /*Xử lý xóa danh mục*/
        {
            var category = _context.Categories.Find(id);
            if (category != null)
            {
                _context.Categories.Remove(category); /*Báo cho EF Core biết object "category" cần bị xóa khỏi bảng database*/
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}