using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Shopbannoithat.Models;

namespace Shopbannoithat.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }
        // Bảng của website
        public DbSet<Product> Products { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Cart> Carts { get; set; }

        public DbSet<Coupon> Coupons { get; set; }

        public DbSet<WishlistItem> Wishlists { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<UserCoupon> UserCoupons { get; set; }

        public DbSet<ProductSize> Sizes { get; set; }
        public DbSet<ProductMaterial> Materials { get; set; }
        public DbSet<ProductColor> Colors { get; set; }

        public DbSet<ProductVariant> ProductVariants { get; set; }
    }
}
