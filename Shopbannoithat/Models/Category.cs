using System.ComponentModel.DataAnnotations;

namespace Shopbannoithat.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, ErrorMessage = "Tên danh mục tối đa 100 ký tự")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [StringLength(100, ErrorMessage = "Mô tả tối đa 500 ký tự")]
        public string? Description { get; set; }

        // Cấp 2: null = danh mục cha (Phòng), có giá trị = danh mục con (Sofa...)
        public int? ParentId { get; set; }
        public Category? Parent { get; set; }
        public List<Category>? Children { get; set; }
    }
}
