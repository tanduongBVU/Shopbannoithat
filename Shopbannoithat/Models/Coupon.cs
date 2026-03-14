using System.ComponentModel.DataAnnotations;

namespace Shopbannoithat.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã giảm giá")]
        [StringLength(50, ErrorMessage = "Mã tối đa 50 ký tự")]
        public string? Code { get; set; } // Mã giảm giá
        [Required(ErrorMessage = "Vui lòng nhập phần trăm giảm")]
        [Range(1, 100, ErrorMessage = "Giảm phải từ 1 đến 100%")]
        public int? Discount { get; set; } // % giảm
        [Required(ErrorMessage = "Vui lòng chọn ngày hết hạn")]
        public DateTime? ExpiryDate { get; set; } // Hạn sử dụng

        public bool IsActive { get; set; } // bật / tắt
    }
}
