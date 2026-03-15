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

        [Required(ErrorMessage = "Please enter a name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter the first address")]
        public string Address1 { get; set; }

        public string Address2 { get; set; }

        [Required(ErrorMessage = "Please enter a city name")]
        public string City { get; set; }

        public string Zip { get; set; }

        public DateTime OrderPlaced { get; set; }
    }
}
