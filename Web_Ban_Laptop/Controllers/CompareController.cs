using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Ban_Laptop.Models;

namespace Web_Ban_Laptop.Controllers
{
    public class CompareController : Controller
    {
        private readonly LaptopStoreDbFinalContext _context;

        public CompareController(LaptopStoreDbFinalContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult AddToCompare(int productId)
        {
            var compareList = HttpContext.Session.GetString("CompareList");
            var productIds = new List<int>();

            if (!string.IsNullOrEmpty(compareList))
            {
                productIds = compareList.Split(',').Select(int.Parse).ToList();
            }

            if (!productIds.Contains(productId) && productIds.Count < 3)
            {
                productIds.Add(productId);
                HttpContext.Session.SetString("CompareList", string.Join(",", productIds));
                return Json(new { success = true, message = "Đã thêm vào danh sách so sánh", count = productIds.Count });
            }
            else if (productIds.Contains(productId))
            {
                return Json(new { success = false, message = "Sản phẩm đã có trong danh sách so sánh" });
            }
            else
            {
                return Json(new { success = false, message = "Chỉ có thể so sánh tối đa 3 sản phẩm" });
            }
        }

        [HttpPost]
        public IActionResult RemoveFromCompare(int productId)
        {
            var compareList = HttpContext.Session.GetString("CompareList");
            if (string.IsNullOrEmpty(compareList))
            {
                return Json(new { success = false });
            }

            var productIds = compareList.Split(',').Select(int.Parse).ToList();
            productIds.Remove(productId);

            if (productIds.Any())
            {
                HttpContext.Session.SetString("CompareList", string.Join(",", productIds));
            }
            else
            {
                HttpContext.Session.Remove("CompareList");
            }

            return Json(new { success = true, count = productIds.Count });
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var compareList = HttpContext.Session.GetString("CompareList");
            if (string.IsNullOrEmpty(compareList))
            {
                ViewBag.Products = new List<Product>();
                return View();
            }

            var productIds = compareList.Split(',').Select(int.Parse).ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.ProductId) && p.IsActive == true)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductDetail)
                .Include(p => p.Supplier)
                .Include(p => p.Feedbacks)
                .ToListAsync();

            // Sắp xếp theo thứ tự trong session
            products = products.OrderBy(p => productIds.IndexOf(p.ProductId)).ToList();

            ViewBag.Products = products;
            return View();
        }
    }
}

