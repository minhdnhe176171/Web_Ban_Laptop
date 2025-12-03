using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Mvc.Core;
using Web_Ban_Laptop.Models;

namespace Web_Ban_Laptop.Controllers
{
    public class ShopController : Controller
    {
        private readonly LaptopStoreDbFinalContext _context;

        public ShopController(LaptopStoreDbFinalContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? page, string? brand, string? priceRange, string? ram, string? cpu, string? sortBy, int? pageSize, string? search)
        {
            int pageNumber = page ?? 1;
            int itemsPerPage = pageSize ?? 12;
            if (itemsPerPage != 12 && itemsPerPage != 16)
                itemsPerPage = 12;

            var query = _context.Products
                .Where(p => p.IsActive == true)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductDetail)
                .Include(p => p.Supplier)
                .Include(p => p.Feedbacks)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => 
                    p.ProductName.Contains(search) || 
                    (p.ShortDescription != null && p.ShortDescription.Contains(search)) ||
                    (p.Supplier != null && p.Supplier.CompanyName.Contains(search)) ||
                    (p.ProductDetail != null && (
                        (p.ProductDetail.Cpu != null && p.ProductDetail.Cpu.Contains(search)) ||
                        (p.ProductDetail.Ram != null && p.ProductDetail.Ram.Contains(search))
                    ))
                );
            }

            // Lọc theo hãng (Supplier)
            if (!string.IsNullOrEmpty(brand))
            {
                query = query.Where(p => p.Supplier != null && p.Supplier.CompanyName.Contains(brand));
            }

            // Lọc theo giá
            if (!string.IsNullOrEmpty(priceRange))
            {
                switch (priceRange)
                {
                    case "under15":
                        query = query.Where(p => (p.DiscountPrice ?? p.Price) < 15000000);
                        break;
                    case "15-25":
                        query = query.Where(p => (p.DiscountPrice ?? p.Price) >= 15000000 && (p.DiscountPrice ?? p.Price) <= 25000000);
                        break;
                    case "over25":
                        query = query.Where(p => (p.DiscountPrice ?? p.Price) > 25000000);
                        break;
                }
            }

            // Lọc theo RAM
            if (!string.IsNullOrEmpty(ram))
            {
                query = query.Where(p => p.ProductDetail != null && p.ProductDetail.Ram != null && p.ProductDetail.Ram.Contains(ram));
            }

            // Lọc theo CPU
            if (!string.IsNullOrEmpty(cpu))
            {
                query = query.Where(p => p.ProductDetail != null && p.ProductDetail.Cpu != null && p.ProductDetail.Cpu.Contains(cpu));
            }

            // Sắp xếp
            switch (sortBy)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.DiscountPrice ?? p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.DiscountPrice ?? p.Price);
                    break;
                case "name_asc":
                    query = query.OrderBy(p => p.ProductName);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(p => p.ProductName);
                    break;
                case "rating":
                    query = query.OrderByDescending(p => p.Feedbacks != null ? p.Feedbacks.Average(f => f.Rating ?? 0) : 0);
                    break;
                default:
                    query = query.OrderByDescending(p => p.CreatedDate);
                    break;
            }

            var totalCount = await query.CountAsync();
            var items = await query.Skip((pageNumber - 1) * itemsPerPage).Take(itemsPerPage).ToListAsync();
            var products = new StaticPagedList<Product>(items, pageNumber, itemsPerPage, totalCount);

            // Lấy danh sách hãng để hiển thị filter
            var brands = await _context.Suppliers
                .Where(s => s.IsActive == true)
                .Select(s => s.CompanyName)
                .Distinct()
                .ToListAsync();

            ViewBag.Brands = brands;
            ViewBag.SelectedBrand = brand;
            ViewBag.SelectedPriceRange = priceRange;
            ViewBag.SelectedRam = ram;
            ViewBag.SelectedCpu = cpu;
            ViewBag.SelectedSort = sortBy;
            ViewBag.PageSize = itemsPerPage;
            ViewBag.Search = search;

            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
            {
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index", new { search = q });
        }

        [HttpGet]
        public async Task<IActionResult> QuickView(int id)
        {
            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(p => p.ProductDetail)
                .Include(p => p.Supplier)
                .Include(p => p.Feedbacks)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.IsActive == true);

            if (product == null)
            {
                return NotFound();
            }

            return PartialView("_QuickView", product);
        }
    }
}
