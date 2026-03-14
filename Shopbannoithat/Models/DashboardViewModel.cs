namespace Shopbannoithat.Models
{
    public class DashboardViewModel
    {
        public decimal TotalRevenue { get; set; }

        public int TotalOrders { get; set; }

        public int TotalProducts { get; set; }

        public List<ProductSales> TopProducts { get; set; }
    }

    public class ProductSales
    {
        public string ProductName { get; set; }

        public int TotalSold { get; set; }
    }
}
