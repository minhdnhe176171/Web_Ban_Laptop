using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Ban_Laptop.Models;

namespace Web_Ban_Laptop.Controllers
{
    public class HomeController : Controller
    {
        private readonly LaptopStoreDbFinalContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(LaptopStoreDbFinalContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            // Top 10 sản phẩm mới nhất
            var newestProducts = await _context.Products
                .Where(p => p.IsActive == true)
                .Include(p => p.ProductImages)
                .Include(p => p.Feedbacks)
                .OrderByDescending(p => p.CreatedDate)
                .Take(10)
                .ToListAsync();

            // Top sản phẩm bán chạy (đếm số lượng bán từ OrderDetails)
            var bestSellingProducts = await _context.OrderDetails
                .GroupBy(od => od.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(od => od.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(10)
                .Join(_context.Products
                    .Where(p => p.IsActive == true)
                    .Include(p => p.ProductImages)
                    .Include(p => p.Feedbacks),
                    x => x.ProductId,
                    p => p.ProductId,
                    (x, p) => p)
                .ToListAsync();

            // Laptop đang giảm giá (DiscountPrice > 0)
            var discountedProducts = await _context.Products
                .Where(p => p.IsActive == true && p.DiscountPrice > 0)
                .Include(p => p.ProductImages)
                .Include(p => p.Feedbacks)
                .OrderByDescending(p => p.DiscountPrice)
                .Take(10)
                .ToListAsync();

            // Lấy danh mục sản phẩm
            var categories = await _context.Categories
                .Include(c => c.Products.Where(p => p.IsActive == true))
                .Where(c => c.Products.Any(p => p.IsActive == true))
                .ToListAsync();

            // Lấy sản phẩm đã xem gần đây từ Session
            var viewedProductsIds = HttpContext.Session.GetString("ViewedProducts");
            List<Product> viewedProducts = null;
            if (!string.IsNullOrEmpty(viewedProductsIds))
            {
                var viewedIds = viewedProductsIds.Split(',').Select(int.Parse).Take(8).ToList();
                viewedProducts = await _context.Products
                    .Where(p => viewedIds.Contains(p.ProductId) && p.IsActive == true)
                    .Include(p => p.ProductImages)
                    .Include(p => p.Feedbacks)
                    .ToListAsync();
                
                // Sắp xếp theo thứ tự trong session
                viewedProducts = viewedProducts.OrderBy(p => viewedIds.IndexOf(p.ProductId)).ToList();
            }

            ViewBag.NewestProducts = newestProducts;
            ViewBag.BestSellingProducts = bestSellingProducts;
            ViewBag.DiscountedProducts = discountedProducts;
            ViewBag.Categories = categories;
            ViewBag.ViewedProducts = viewedProducts ?? new List<Product>();

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
