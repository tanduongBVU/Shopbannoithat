using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbannoithat.Data;
using Shopbannoithat.Models;


namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    public class DashboardController : Controller
    {

        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new DashboardViewModel();

            model.TotalProducts = _context.Products.Count();

            model.TotalOrders = _context.Orders.Count();

            model.TotalRevenue = _context.OrderDetails
                .Sum(x => x.Price * x.Quantity);

            model.TopProducts = _context.OrderDetails
    .Join(_context.Products,
        od => od.ProductId,
        p => p.Id,
        (od, p) => new { od, p })
    .GroupBy(x => x.p.Name)
    .Select(g => new ProductSales
    {
        ProductName = g.Key,
        TotalSold = g.Sum(x => x.od.Quantity)
    })
    .OrderByDescending(x => x.TotalSold)
    .Take(5)
    .ToList();

            return View(model);
        }
    }
}
