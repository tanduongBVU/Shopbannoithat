using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopbannoithat.Data;
using System.Linq;

namespace Shopbannoithat.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ExportController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExportController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang chọn thời gian
        public IActionResult Index()
        {
            return View();
        }

        // Xuất Excel
        [HttpPost]
        public IActionResult ExportExcel(DateTime fromDate, DateTime toDate)
        {
            toDate = toDate.Date.AddDays(1).AddTicks(-1);

            var orders = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate)
                .ToList();

            // Lấy danh sách user riêng
            var userIds = orders.Select(o => o.UserId).Distinct().ToList();
            var users = _context.Users
                .Where(u => userIds.Contains(u.Id.ToString()))
                .ToList();

            using var wb = new XLWorkbook();

            // ========== SHEET 1: DOANH THU ==========
            var ws1 = wb.Worksheets.Add("Doanh Thu");

            ws1.Cell(1, 1).Value = "THỐNG KÊ DOANH THU";
            ws1.Range(1, 1, 1, 4).Merge().Style
                .Font.SetBold(true)
                .Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws1.Cell(2, 1).Value = $"Từ ngày: {fromDate:dd/MM/yyyy} - Đến ngày: {toDate:dd/MM/yyyy}";
            ws1.Range(2, 1, 2, 4).Merge().Style
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws1.Cell(4, 1).Value = "Ngày";
            ws1.Cell(4, 2).Value = "Số đơn hàng";
            ws1.Cell(4, 3).Value = "Doanh thu";
            ws1.Cell(4, 4).Value = "Đã hoàn thành";

            ws1.Range(4, 1, 4, 4).Style
                .Font.SetBold(true)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#6b4f3b"))
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var doanhThuTheoNgay = orders
                .GroupBy(o => o.OrderDate.Date)
                .OrderBy(g => g.Key)
                .ToList();

            int row1 = 5;
            decimal tongDoanhThu = 0;
            foreach (var group in doanhThuTheoNgay)
            {
                var doanhThu = group.Sum(o => o.TotalPrice);
                tongDoanhThu += doanhThu;

                ws1.Cell(row1, 1).Value = group.Key.ToString("dd/MM/yyyy");
                ws1.Cell(row1, 2).Value = group.Count();
                ws1.Cell(row1, 3).Value = doanhThu;
                ws1.Cell(row1, 3).Style.NumberFormat.Format = "#,##0";
                ws1.Cell(row1, 4).Value = group.Count(o => o.Status == "Completed");

                if (row1 % 2 == 0)
                    ws1.Range(row1, 1, row1, 4).Style
                        .Fill.SetBackgroundColor(XLColor.FromHtml("#f5f0eb"));
                row1++;
            }

            ws1.Cell(row1, 1).Value = "TỔNG CỘNG";
            ws1.Cell(row1, 2).Value = orders.Count;
            ws1.Cell(row1, 3).Value = tongDoanhThu;
            ws1.Cell(row1, 3).Style.NumberFormat.Format = "#,##0";
            ws1.Range(row1, 1, row1, 4).Style
                .Font.SetBold(true)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#6b4f3b"))
                .Font.SetFontColor(XLColor.White);

            ws1.Columns().AdjustToContents();

            // ========== SHEET 2: SẢN PHẨM BÁN CHẠY ==========
            var ws2 = wb.Worksheets.Add("San Pham Ban Chay");

            ws2.Cell(1, 1).Value = "SẢN PHẨM BÁN CHẠY";
            ws2.Range(1, 1, 1, 4).Merge().Style
                .Font.SetBold(true)
                .Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws2.Cell(3, 1).Value = "STT";
            ws2.Cell(3, 2).Value = "Tên sản phẩm";
            ws2.Cell(3, 3).Value = "Số lượng đã bán";
            ws2.Cell(3, 4).Value = "Doanh thu";

            ws2.Range(3, 1, 3, 4).Style
                .Font.SetBold(true)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#6b4f3b"))
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var sanPhamBanChay = orders
                .SelectMany(o => o.OrderDetails)
                .GroupBy(od => od.Product?.Name ?? "Không xác định")
                .Select(g => new
                {
                    TenSP = g.Key,
                    SoLuong = g.Sum(od => od.Quantity),
                    DoanhThu = g.Sum(od => od.Quantity * od.Price)
                })
                .OrderByDescending(x => x.SoLuong)
                .ToList();

            int row2 = 4, stt2 = 1;
            foreach (var sp in sanPhamBanChay)
            {
                ws2.Cell(row2, 1).Value = stt2++;
                ws2.Cell(row2, 2).Value = sp.TenSP;
                ws2.Cell(row2, 3).Value = sp.SoLuong;
                ws2.Cell(row2, 4).Value = sp.DoanhThu;
                ws2.Cell(row2, 4).Style.NumberFormat.Format = "#,##0";

                if (row2 % 2 == 0)
                    ws2.Range(row2, 1, row2, 4).Style
                        .Fill.SetBackgroundColor(XLColor.FromHtml("#f5f0eb"));
                row2++;
            }

            ws2.Columns().AdjustToContents();

            // ========== SHEET 3: KHÁCH HÀNG ==========
            var ws3 = wb.Worksheets.Add("Khach Hang");

            ws3.Cell(1, 1).Value = "THỐNG KÊ KHÁCH HÀNG";
            ws3.Range(1, 1, 1, 5).Merge().Style
                .Font.SetBold(true)
                .Font.SetFontSize(14)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            ws3.Cell(3, 1).Value = "STT";
            ws3.Cell(3, 2).Value = "Tên khách hàng";
            ws3.Cell(3, 3).Value = "Email";
            ws3.Cell(3, 4).Value = "Số đơn hàng";
            ws3.Cell(3, 5).Value = "Tổng chi tiêu";

            ws3.Range(3, 1, 3, 5).Style
                .Font.SetBold(true)
                .Fill.SetBackgroundColor(XLColor.FromHtml("#6b4f3b"))
                .Font.SetFontColor(XLColor.White)
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            var khachHang = orders
                .GroupBy(o => o.UserId)
                .Select(g =>
                {
                    // mới - thêm .ToString()
                    var user = users.FirstOrDefault(u => u.Id.ToString() == g.Key);
                    return new
                    {
                        Ten = g.First().Name, // lấy tên từ đơn hàng
                        Email = user?.Email ?? "",
                        SoDon = g.Count(),
                        TongChiTieu = g.Sum(o => o.TotalPrice)
                    };
                })
                .OrderByDescending(x => x.TongChiTieu)
                .ToList();

            int row3 = 4, stt3 = 1;
            foreach (var kh in khachHang)
            {
                ws3.Cell(row3, 1).Value = stt3++;
                ws3.Cell(row3, 2).Value = kh.Ten;
                ws3.Cell(row3, 3).Value = kh.Email;
                ws3.Cell(row3, 4).Value = kh.SoDon;
                ws3.Cell(row3, 5).Value = kh.TongChiTieu;
                ws3.Cell(row3, 5).Style.NumberFormat.Format = "#,##0";

                if (row3 % 2 == 0)
                    ws3.Range(row3, 1, row3, 5).Style
                        .Fill.SetBackgroundColor(XLColor.FromHtml("#f5f0eb"));
                row3++;
            }

            ws3.Columns().AdjustToContents();

            // Xuất file
            using var stream = new MemoryStream();
            wb.SaveAs(stream);

            string fileName = $"ThongKe_{fromDate:ddMMyyyy}_{toDate:ddMMyyyy}.xlsx";
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }
}
