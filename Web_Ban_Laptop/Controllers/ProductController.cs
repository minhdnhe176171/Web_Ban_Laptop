using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Ban_Laptop.Models;

namespace Web_Ban_Laptop.Controllers
{
    public class ProductController : Controller
    {
        private readonly LaptopStoreDbFinalContext _context;

        public ProductController(LaptopStoreDbFinalContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductDetail)
                .Include(p => p.Supplier)
                .Include(p => p.Feedbacks)
                    .ThenInclude(f => f.User)
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive == true);

            if (product == null)
            {
                return NotFound();
            }

            // Lưu sản phẩm đã xem vào Session
            var viewedProducts = HttpContext.Session.GetString("ViewedProducts");
            var productIds = new List<int>();
            
            if (!string.IsNullOrEmpty(viewedProducts))
            {
                productIds = viewedProducts.Split(',').Select(int.Parse).ToList();
            }

            if (!productIds.Contains(id))
            {
                productIds.Insert(0, id);
                if (productIds.Count > 10)
                {
                    productIds = productIds.Take(10).ToList();
                }
                HttpContext.Session.SetString("ViewedProducts", string.Join(",", productIds));
            }

            // Lấy sản phẩm liên quan (cùng category)
            var relatedProducts = await _context.Products
                .Where(p => p.IsActive == true && 
                           p.CategoryId == product.CategoryId && 
                           p.ProductId != id)
                .Include(p => p.ProductImages)
                .Include(p => p.Feedbacks)
                .Take(8)
                .ToListAsync();

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }
    }
}
