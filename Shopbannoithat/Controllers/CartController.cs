using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Shopbannoithat.Data;
using Shopbannoithat.Models;

namespace Shopbannoithat.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị giỏ hàng
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // Thêm sản phẩm
        public IActionResult Add(int id)
        {
            var product = _context.Products.Find(id);

            if (product == null) return NotFound();

            var cart = GetCart();

            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                item.Quantity++;
            }
            else
            {
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Image = product.Image,
                    Quantity = 1
                });
            }

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        // Xóa sản phẩm
        public IActionResult Remove(int id)
        {
            var cart = GetCart();

            cart.RemoveAll(x => x.ProductId == id);

            SaveCart(cart);

            return RedirectToAction("Index");
        }

        // Lấy giỏ hàng từ session
        private List<CartItem> GetCart()
        {
            var session = HttpContext.Session.GetString("Cart");

            if (session != null)
            {
                return JsonConvert.DeserializeObject<List<CartItem>>(session);
            }

            return new List<CartItem>();
        }

        // Lưu giỏ hàng
        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString("Cart",
                JsonConvert.SerializeObject(cart));
        }
    }
}
