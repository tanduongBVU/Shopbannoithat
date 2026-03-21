namespace Shopbannoithat.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public int? VariantId { get; set; }
        public ProductVariant Variant { get; set; }
        public decimal Price { get; set; }


        public Order Order { get; set; }

        public Product Product { get; set; }

        public int? SizeId { get; set; }
        public int? MaterialId { get; set; }
        public int? ColorId { get; set; }

        // 🔥 Snapshot biến thể
        public string? VariantSize { get; set; }
        public string? VariantMaterial { get; set; }
        public string? VariantColor { get; set; }


    }
}
