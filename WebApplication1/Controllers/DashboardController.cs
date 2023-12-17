using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Models;
using WebApplication1.Utilities;

namespace WebApplication1.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        private const string CartSessionKey = "Cart";

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DashboardViewModel
            {
                Products = await _context.Product.ToListAsync(),
                CartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> AddToCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var product = await GetProductById(productId);

            if (product != null)
            {
                var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);
                if (existingItem != null)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    cart.Add(new CartItem
                    {
                        ProductId = product.ProductId,
                        ProductName = product.Name,
                        Price = product.Price,
                        Quantity = 1
                    });
                }

                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);

                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        private async Task<Product?> GetProductById(int productId)
        {
            return await _context.Product.FirstOrDefaultAsync(m => m.ProductId == productId);
        }

        [HttpDelete]
        public JsonResult RemoveFromCart(int productId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var itemToRemove = cart.FirstOrDefault(item => item.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        [HttpPut]
        public JsonResult UpdateQuantity(int productId, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();

            var item = cart.FirstOrDefault(item => item.ProductId == productId);
            if (item != null && quantity > 0)
            {
                item.Quantity = quantity;
                HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
                return Json(new { success = true });
            }

            return Json(new { success = false });
        }

        public IActionResult Checkout()
        {
            var cartItems = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            return View(cartItems);
        }

    }
}
