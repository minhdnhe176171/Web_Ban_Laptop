using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Ban_Laptop.Models;
using Web_Ban_Laptop.Services;

namespace Web_Ban_Laptop.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly LaptopStoreDbFinalContext _context;

        public CartController(CartService cartService, LaptopStoreDbFinalContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCart(HttpContext.Session);
            
            // Cập nhật thông tin sản phẩm từ database
            foreach (var item in cart)
            {
                var product = _context.Products.Find(item.ProductId);
                if (product != null)
                {
                    item.Price = product.Price;
                    item.DiscountPrice = product.DiscountPrice;
                }
            }
            
            _cartService.SaveCart(HttpContext.Session, cart);
            
            ViewBag.CartTotal = _cartService.GetCartTotal(HttpContext.Session);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || product.IsActive != true)
            {
                return Json(new { success = false, message = "Sản phẩm không tồn tại" });
            }

            _cartService.AddToCart(HttpContext.Session, product, quantity);
            
            return Json(new { 
                success = true, 
                message = "Đã thêm vào giỏ hàng",
                cartCount = _cartService.GetCartCount(HttpContext.Session)
            });
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            _cartService.UpdateCartItem(HttpContext.Session, productId, quantity);
            
            var cart = _cartService.GetCart(HttpContext.Session);
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            
            return Json(new { 
                success = true,
                subTotal = item?.SubTotal ?? 0,
                cartTotal = _cartService.GetCartTotal(HttpContext.Session),
                cartCount = _cartService.GetCartCount(HttpContext.Session)
            });
        }

        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            _cartService.RemoveFromCart(HttpContext.Session, productId);
            
            return Json(new { 
                success = true,
                cartTotal = _cartService.GetCartTotal(HttpContext.Session),
                cartCount = _cartService.GetCartCount(HttpContext.Session)
            });
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart(HttpContext.Session);
            return RedirectToAction("Index");
        }
    }
}

