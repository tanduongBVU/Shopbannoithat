namespace Shopbannoithat.Models
{
    public class ProductVariant
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int? SizeId { get; set; }
        public ProductSize? Size { get; set; }

        public int? MaterialId { get; set; }
        public ProductMaterial? Material { get; set; }

        public int? ColorId { get; set; }
        public ProductColor? Color { get; set; }

        public decimal Price { get; set; }
        public int Stock { get; set; } // Số lượng tồn kho theo variant

        // 🔥 THÊM DÒNG NÀY
        public bool IsDeleted { get; set; } = false;
    }
}
