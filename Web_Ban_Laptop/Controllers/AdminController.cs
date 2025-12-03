using Microsoft.AspNetCore.Mvc;
using Web_Ban_Laptop.Services;

namespace Web_Ban_Laptop.Controllers
{
    public class AdminController : Controller
    {
        private readonly ProductImageSeeder _seeder;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            ProductImageSeeder seeder,
            ILogger<AdminController> logger)
        {
            _seeder = seeder;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> SeedProductImages()
        {
            try
            {
                _logger.LogInformation("🔄 Bắt đầu seed ProductImages từ Admin Controller...");
                await _seeder.SeedProductImagesAsync();
                return Json(new { success = true, message = "Seed ProductImages thành công! Xem Console/Logs để biết chi tiết." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi seed ProductImages từ Admin Controller");
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
    }
}

