namespace Shopbannoithat.Models
{
    public class Cart
    {
        public int Id { get; set; }

        public string UserEmail { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public Product Product { get; set; }

        public int? SizeId { get; set; }
        public int? MaterialId { get; set; }
        public int? ColorId { get; set; }


    }
}
