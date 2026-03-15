namespace Shopbannoithat.Models
{
    public class WishlistItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }

        public string UserId { get; set; }
        public string Name { get; set; }

        public decimal Price { get; set; }

        public string Image { get; set; }

        public Product Product { get; set; }
    }
}
