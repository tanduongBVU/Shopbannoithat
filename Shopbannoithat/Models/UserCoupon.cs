namespace Shopbannoithat.Models
{
    public class UserCoupon
    {
        public int Id { get; set; }

        public string UserEmail { get; set; }  // hoặc UserId
        public int CouponId { get; set; }

        public DateTime UsedDate { get; set; }
    }
}
