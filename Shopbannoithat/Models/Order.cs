using System.ComponentModel.DataAnnotations;

namespace Shopbannoithat.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalPrice { get; set; }

        public string Status { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }

        public bool Shipped { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tổ ấp")]
        public string Address1 { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập xã phường")]
        public string Address2 { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thành phố ")]
        public string City { get; set; }

        

        public DateTime OrderPlaced { get; set; }

        public string? CouponCode { get; set; }
        public int? DiscountPercent { get; set; }


    }
}
