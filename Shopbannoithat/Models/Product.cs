using System.ComponentModel.DataAnnotations;
using System.Drawing;

namespace Shopbannoithat.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm tối đa 100 ký tự")]
        public string Name { get; set; }

        
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [StringLength(500, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        public string? Description { get; set; }

        
        public string? Image { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số lượng")]
        [Range(0, 100000, ErrorMessage = "Số lượng không hợp lệ")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại sản phẩm")]
        public int? CategoryId { get; set; }

        public Category? Category { get; set; }

        // Thêm mới
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔥 Thêm mới
        
        public string? SKU { get; set; }
        //// Thay bằng foreign key
        //public int? SizeId { get; set; }
        //public ProductSize? Size { get; set; }

        //public int? MaterialId { get; set; }
        //public ProductMaterial? Material { get; set; }

        //public int? ColorId { get; set; }
        //public ProductColor? Color { get; set; }

        public List<ProductVariant>? Variants { get; set; }


    }
}
