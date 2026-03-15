using System.ComponentModel.DataAnnotations;

namespace Shopbannoithat.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm tối đa 100 ký tự")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá sản phẩm")]
        [Range(1, 1000000000, ErrorMessage = "Giá phải lớn hơn 0")]
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
    }
}
