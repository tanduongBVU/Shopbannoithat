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
            // Sản phẩm bán chạy
            var bestSelling = await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new { ProductId = g.Key, TotalSold = g.Sum(od => od.Quantity) })
                .OrderByDescending(x => x.TotalSold)
                .Take(8)
                .Join(_context.Products.Include(p => p.Category),
                    sold => sold.ProductId,
                    product => product.Id,
                    (sold, product) => product)
                .ToListAsync();

            if (!bestSelling.Any())
            {
                bestSelling = await _context.Products
                    .Include(p => p.Category)
                    .OrderByDescending(p => p.Id)
                    .Take(8)
                    .ToListAsync();
            }

            // Phòng khách
            var phongKhach = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Category != null && p.Category.Name.ToLower().Contains("khách"))
                .Take(8)
                .ToListAsync();

            // Phòng ngủ
            var phongNgu = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Category != null && p.Category.Name.ToLower().Contains("ngủ"))
                .Take(8)
                .ToListAsync();

            // Phòng bếp
            var phongBep = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.Category != null && p.Category.Name.ToLower().Contains("bếp"))
                .Take(8)
                .ToListAsync();

            ViewBag.BestSelling = bestSelling;
            ViewBag.PhongKhach = phongKhach;
            ViewBag.PhongNgu = phongNgu;
            ViewBag.PhongBep = phongBep;

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
    }
}