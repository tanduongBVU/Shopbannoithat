namespace Shopbannoithat.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        public string Code { get; set; } // Mã giảm giá

        public int Discount { get; set; } // % giảm

        public DateTime ExpiryDate { get; set; } // Hạn sử dụng

        public bool IsActive { get; set; } // bật / tắt
    }
}
