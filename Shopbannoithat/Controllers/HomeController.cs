using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Helper: lấy tất cả ID danh mục con theo tên phòng cha
            List<int> GetCategoryIds(string roomName)
            {
                var parent = _context.Categories
                    .FirstOrDefault(c => c.Name == roomName && c.ParentId == null);
                if (parent == null) return new List<int>();
                var childIds = _context.Categories
                    .Where(c => c.ParentId == parent.Id)
                    .Select(c => c.Id)
                    .ToList();
                childIds.Add(parent.Id);
                return childIds;
            }

            // Sản phẩm bán chạy
            var bestSelling = await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(od => od.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(8)
                .Join(_context.Products
                    .Include(p => p.Category).ThenInclude(c => c.Parent)
                    .Include(p => p.Variants),
                    sold => sold.ProductId,
                    product => product.Id,
                    (sold, product) => product)
                .ToListAsync();

           

            var idsKhach = GetCategoryIds("Phòng khách");
            var idsNgu = GetCategoryIds("Phòng ngủ");
            var idsBep = GetCategoryIds("Phòng bếp");
            var idsLamViec = GetCategoryIds("Phòng làm việc");

            var phongKhach = await _context.Products
                .Include(p => p.Category).ThenInclude(c => c.Parent)
                .Include(p => p.Variants)
                .Where(p => p.CategoryId != null && idsKhach.Contains(p.CategoryId.Value))
                .Take(8).ToListAsync();

            var phongNgu = await _context.Products
                .Include(p => p.Category).ThenInclude(c => c.Parent)
                .Include(p => p.Variants)
                .Where(p => p.CategoryId != null && idsNgu.Contains(p.CategoryId.Value))
                .Take(8).ToListAsync();

            var phongBep = await _context.Products
                .Include(p => p.Category).ThenInclude(c => c.Parent)
                .Include(p => p.Variants)
                .Where(p => p.CategoryId != null && idsBep.Contains(p.CategoryId.Value))
                .Take(8).ToListAsync();

            var phongLamViec = await _context.Products
                .Include(p => p.Category).ThenInclude(c => c.Parent)
                .Include(p => p.Variants)
                .Where(p => p.CategoryId != null && idsLamViec.Contains(p.CategoryId.Value))
                .Take(8).ToListAsync();

            var sanPhamMoi = await _context.Products
    .Include(p => p.Category).ThenInclude(c => c.Parent)
    .Include(p => p.Variants)
    .OrderByDescending(p => p.CreatedAt)
    .Take(8)
    .ToListAsync();

            ViewBag.SanPhamMoi = sanPhamMoi;
            ViewBag.BestSelling = bestSelling;
            ViewBag.PhongKhach = phongKhach;
            
            ViewBag.PhongNgu = phongNgu;
            ViewBag.PhongBep = phongBep;
            ViewBag.PhongLamViec = phongLamViec;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult About()
        {
            return View();
        }

        
    }
}