using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Ban_Laptop.Models;

namespace Web_Ban_Laptop.Controllers
{
    public class WishlistController : Controller
    {
        private readonly LaptopStoreDbFinalContext _context;

        public WishlistController(LaptopStoreDbFinalContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Toggle(int productId)
        {
            var wishlist = HttpContext.Session.GetString("Wishlist");
            var productIds = new List<int>();

            if (!string.IsNullOrEmpty(wishlist))
            {
                productIds = wishlist.Split(',').Select(int.Parse).ToList();
            }

            if (productIds.Contains(productId))
            {
                productIds.Remove(productId);
                HttpContext.Session.SetString("Wishlist", string.Join(",", productIds));
                return Json(new { success = true, isInWishlist = false, message = "Đã xóa khỏi yêu thích", count = productIds.Count });
            }
            else
            {
                productIds.Add(productId);
                HttpContext.Session.SetString("Wishlist", string.Join(",", productIds));
                return Json(new { success = true, isInWishlist = true, message = "Đã thêm vào yêu thích", count = productIds.Count });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var wishlist = HttpContext.Session.GetString("Wishlist");
            if (string.IsNullOrEmpty(wishlist))
            {
                ViewBag.Products = new List<Product>();
                return View();
            }

            var productIds = wishlist.Split(',').Select(int.Parse).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId) && p.IsActive == true)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductDetail)
                .Include(p => p.Supplier)
                .Include(p => p.Feedbacks)
                .ToListAsync();

            ViewBag.Products = products;
            return View();
        }
    }
}

