using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Web_Ban_Laptop.Models;

namespace Web_Ban_Laptop.Services;

public class ProductImageSeeder
{
    private readonly LaptopStoreDbFinalContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ProductImageSeeder> _logger;
    private readonly IConfiguration _configuration;

    public ProductImageSeeder(
        LaptopStoreDbFinalContext context,
        IWebHostEnvironment environment,
        ILogger<ProductImageSeeder> logger,
        IConfiguration configuration)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SeedProductImagesAsync()
    {
        try
        {
            _logger.LogInformation("🖼️ Bắt đầu seed ProductImages...");
            _logger.LogInformation("📂 WebRootPath: {WebRootPath}", _environment.WebRootPath);

            // Đường dẫn đến thư mục products (wwwroot/products/laptops)
            var productsImagePath = Path.Combine(_environment.WebRootPath, "products", "laptops");
            _logger.LogInformation("📂 Đường dẫn products: {Path}", productsImagePath);
            
            if (!Directory.Exists(productsImagePath))
            {
                _logger.LogWarning("⚠️ Thư mục {Path} không tồn tại. Tạo thư mục mới...", productsImagePath);
                try
                {
                    Directory.CreateDirectory(productsImagePath);
                    _logger.LogInformation("✅ Đã tạo thư mục: {Path}", productsImagePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Không thể tạo thư mục: {Path}", productsImagePath);
                    return;
                }
            }
            else
            {
                _logger.LogInformation("✅ Thư mục tồn tại: {Path}", productsImagePath);
            }

            // Lấy tất cả products từ database
            var products = await _context.Products
                .Where(p => p.IsActive == true)
                .ToListAsync();

            _logger.LogInformation("📦 Tìm thấy {Count} sản phẩm trong database", products.Count);
            
            // Kiểm tra số lượng ảnh hiện có trong database
            var existingImageCount = await _context.ProductImages.CountAsync();
            _logger.LogInformation("📊 Số lượng ảnh hiện có trong database: {Count}", existingImageCount);
            
            // Kiểm tra xem có cần xóa ảnh cũ không (từ config)
            var forceUpdate = _configuration.GetValue<bool>("ForceUpdateProductImages", false);
            if (forceUpdate && existingImageCount > 0)
            {
                _logger.LogWarning("⚠️ ForceUpdate = true, sẽ xóa tất cả ảnh cũ và thêm lại...");
                var deletedCount = await _context.ProductImages.ExecuteDeleteAsync();
                _logger.LogInformation("🗑️ Đã xóa {Count} ảnh cũ từ bảng ProductImages", deletedCount);
                
                // Reset Thumbnail về null cho tất cả products để cập nhật lại
                await _context.Products.ExecuteUpdateAsync(p => p.SetProperty(x => x.Thumbnail, (string?)null));
                _logger.LogInformation("🔄 Đã reset Thumbnail về null cho tất cả products");
                
                await _context.SaveChangesAsync();
            }

            int addedCount = 0;
            int updatedCount = 0;
            int skippedCount = 0;
            int folderProcessed = 0;

            // Kiểm tra xem có file ảnh trực tiếp trong thư mục products không
            var directImageFiles = Directory.GetFiles(productsImagePath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" }
                    .Contains(Path.GetExtension(f).ToLowerInvariant()))
                .ToList();
            
            _logger.LogInformation("📁 Tìm thấy {Count} file ảnh trực tiếp trong thư mục products", directImageFiles.Count);

            // Tìm tất cả folder ProductID_X trực tiếp trong thư mục laptops (không đệ quy)
            var allFolders = new List<string>();
            try
            {
                allFolders = Directory.GetDirectories(productsImagePath, "*", SearchOption.TopDirectoryOnly).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi đọc thư mục: {Path}", productsImagePath);
                return;
            }
            
            _logger.LogInformation("📂 Tìm thấy {Count} folder trong {Path}", allFolders.Count, productsImagePath);
            
            if (allFolders.Count > 0)
            {
                _logger.LogInformation("📋 Danh sách folder tìm được:");
                foreach (var folder in allFolders.Take(20)) // Log 20 folder đầu tiên
                {
                    var folderName = Path.GetFileName(folder);
                    var fileCount = Directory.GetFiles(folder, "*.*", SearchOption.TopDirectoryOnly).Length;
                    _logger.LogInformation("   - {FolderName} ({FileCount} files)", folderName, fileCount);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ Không tìm thấy folder nào trong {Path}", productsImagePath);
                _logger.LogWarning("⚠️ Vui lòng kiểm tra:");
                _logger.LogWarning("   1. Thư mục có tồn tại không: {Path}", productsImagePath);
                _logger.LogWarning("   2. Có folder ProductID_X nào không (ví dụ: ProductID_1, ProductID_2)");
                _logger.LogWarning("   3. Quyền truy cập thư mục");
                return;
            }

            // Tìm tất cả các folder có pattern ProductID_X (hỗ trợ nhiều format)
            var productFolders = allFolders
                .Where(dir => 
                {
                    var folderName = Path.GetFileName(dir);
                    // Hỗ trợ: ProductID_1, ProductId_1, productid_1, ProductID-1, ProductID 1, ProductID1
                    var isMatch = System.Text.RegularExpressions.Regex.IsMatch(
                        folderName, 
                        @"^productid[_\s-]?(\d+)$|^productid(\d+)$", 
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    
                    if (isMatch)
                    {
                        _logger.LogDebug("✅ Folder match pattern: {FolderName}", folderName);
                    }
                    
                    return isMatch;
                })
                .ToList();

            _logger.LogInformation("📁 Tìm thấy {Count} folder ProductID (sau khi filter)", productFolders.Count);
            
            if (productFolders.Count == 0)
            {
                _logger.LogWarning("⚠️ Không tìm thấy folder nào có pattern ProductID_X!");
                _logger.LogWarning("⚠️ Vui lòng kiểm tra tên folder. Pattern hỗ trợ: ProductID_1, ProductId_1, productid_1, ProductID-1, ProductID 1, ProductID1");
                return;
            }

            // Xử lý từng folder ProductID_X
            foreach (var folderPath in productFolders)
            {
                try
                {
                    var folderName = Path.GetFileName(folderPath);
                    
                    // Parse ProductID từ tên folder - hỗ trợ nhiều format
                    int productId = 0;
                    var match1 = System.Text.RegularExpressions.Regex.Match(folderName, @"productid[_\s-]?(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    var match2 = System.Text.RegularExpressions.Regex.Match(folderName, @"productid(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    
                    if (match1.Success && int.TryParse(match1.Groups[1].Value, out productId))
                    {
                        // OK
                    }
                    else if (match2.Success && int.TryParse(match2.Groups[1].Value, out productId))
                    {
                        // OK
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Không thể parse ProductID từ folder: {FolderName}", folderName);
                        skippedCount++;
                        continue;
                    }
                    
                    _logger.LogInformation("🔍 Parse được ProductID: {ProductId} từ folder: {FolderName}", productId, folderName);

                    // Tìm product tương ứng
                    var product = products.FirstOrDefault(p => p.ProductId == productId);
                    if (product == null)
                    {
                        _logger.LogWarning("⚠️ Không tìm thấy Product với ID: {ProductId} trong database (folder: {FolderName})", 
                            productId, folderName);
                        skippedCount++;
                        continue;
                    }

                    // Lấy tất cả file ảnh trong folder này
                    var imageFiles = GetImageFilesInFolder(folderPath);
                    
                    if (imageFiles.Count == 0)
                    {
                        _logger.LogWarning("⚠️ Không tìm thấy ảnh nào trong folder: {FolderName}", folderName);
                        skippedCount++;
                        continue;
                    }

                    _logger.LogInformation("📂 Xử lý folder {FolderName} (ProductID: {ProductId}, Product: {ProductName}) - {ImageCount} ảnh", 
                        folderName, productId, product.ProductName, imageFiles.Count);

                    // Sắp xếp ảnh theo tên để có thứ tự nhất quán
                    imageFiles = imageFiles.OrderBy(f => Path.GetFileName(f)).ToList();

                    // Xử lý từng ảnh trong folder
                    for (int i = 0; i < imageFiles.Count; i++)
                    {
                        try
                        {
                            var imageFile = imageFiles[i];
                            var relativePath = GetRelativePath(imageFile, _environment.WebRootPath);

                            _logger.LogInformation("🔍 Đang xử lý ảnh: {ImageFile}", Path.GetFileName(imageFile));
                            _logger.LogInformation("🔍 RelativePath: {RelativePath}", relativePath);

                            // Kiểm tra xem ảnh đã tồn tại chưa (case-insensitive)
                            var existingImages = await _context.ProductImages
                                .Where(pi => pi.ProductId == product.ProductId)
                                .ToListAsync();
                            
                            var existingImage = existingImages
                                .FirstOrDefault(pi => pi.ImageUrl.Equals(relativePath, StringComparison.OrdinalIgnoreCase));

                            if (existingImage != null)
                            {
                                _logger.LogWarning("⏭️ Ảnh đã tồn tại: {ImageUrl} cho Product {ProductId} (ImageID: {ImageId})", 
                                    relativePath, product.ProductId, existingImage.ImageId);
                                _logger.LogWarning("   DB ImageUrl: {DbImageUrl}", existingImage.ImageUrl);
                                continue;
                            }
                            
                            // Log nếu không tìm thấy existing
                            _logger.LogInformation("   ✅ Ảnh chưa tồn tại, sẽ thêm mới");

                            // SortOrder = index + 1 (ảnh đầu tiên = 1)
                            int sortOrder = i + 1;

                            // Tạo ProductImage mới
                            var productImage = new ProductImage
                            {
                                ProductId = product.ProductId,
                                ImageUrl = relativePath,
                                SortOrder = sortOrder
                            };

                            _context.ProductImages.Add(productImage);
                            addedCount++;

                            _logger.LogInformation("✅ [ADDED] Thêm ảnh [{SortOrder}]: {ImageUrl} cho Product: {ProductName} (ID: {ProductId})", 
                                sortOrder, relativePath, product.ProductName, product.ProductId);

                            // Cập nhật Thumbnail nếu là ảnh đầu tiên (luôn cập nhật, không cần check empty)
                            if (sortOrder == 1)
                            {
                                var oldThumbnail = product.Thumbnail;
                                product.Thumbnail = relativePath;
                                _context.Products.Update(product);
                                updatedCount++;
                                _logger.LogInformation("🖼️ Cập nhật Thumbnail cho Product {ProductId}: {OldThumbnail} -> {NewThumbnail}", 
                                    product.ProductId, oldThumbnail ?? "(null)", relativePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ Lỗi khi xử lý ảnh: {ImageFile}", imageFiles[i]);
                        }
                    }
                    
                    // Lưu thay đổi sau mỗi folder để đảm bảo không mất dữ liệu
                    try
                    {
                        var saved = await _context.SaveChangesAsync();
                        _logger.LogInformation("💾 Đã lưu {Count} thay đổi cho folder {FolderName}", saved, folderName);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Lỗi khi lưu thay đổi cho folder {FolderName}", folderName);
                        throw;
                    }

                    folderProcessed++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Lỗi khi xử lý folder: {FolderPath}", folderPath);
                    skippedCount++;
                }
            }

            // Lưu thay đổi lần cuối (nếu còn)
            try
            {
                var finalSaved = await _context.SaveChangesAsync();
                _logger.LogInformation("💾 Lưu thay đổi cuối cùng: {Count} records", finalSaved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Lỗi khi lưu thay đổi cuối cùng");
                throw;
            }

            _logger.LogInformation("✅ Hoàn thành seed ProductImages!");
            _logger.LogInformation("📊 Thống kê:");
            _logger.LogInformation("   - Đã xử lý: {FolderCount} folder", folderProcessed);
            _logger.LogInformation("   - Đã thêm: {AddedCount} ảnh", addedCount);
            _logger.LogInformation("   - Đã cập nhật: {UpdatedCount} thumbnail", updatedCount);
            _logger.LogInformation("   - Đã bỏ qua: {SkippedCount} folder/file", skippedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Lỗi khi seed ProductImages");
            throw;
        }
    }

    private List<string> GetImageFilesInFolder(string folderPath)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg" };
        var imageFiles = new List<string>();

        try
        {
            // Lấy tất cả file ảnh trong folder (không đệ quy vào subfolder)
            var files = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(f => imageExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
                .Where(f => !Path.GetFileName(f).Equals("placeholder.svg", StringComparison.OrdinalIgnoreCase))
                .ToList();

            imageFiles.AddRange(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Lỗi khi đọc thư mục: {Path}", folderPath);
        }

        return imageFiles;
    }





    private string GetRelativePath(string fullPath, string webRootPath)
    {
        try
        {
            // Lấy đường dẫn relative từ webRootPath
            var relativePath = Path.GetRelativePath(webRootPath, fullPath);
            
            // Chuyển đổi backslash thành forward slash (cho web)
            relativePath = relativePath.Replace('\\', '/');
            
            // Đảm bảo bắt đầu bằng /
            if (!relativePath.StartsWith("/"))
            {
                relativePath = "/" + relativePath;
            }
            
            _logger.LogDebug("📁 FullPath: {FullPath}", fullPath);
            _logger.LogDebug("📁 WebRootPath: {WebRootPath}", webRootPath);
            _logger.LogDebug("📁 RelativePath: {RelativePath}", relativePath);

            return relativePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Lỗi khi tạo relative path từ {FullPath}", fullPath);
            // Fallback: tạo relative path thủ công
            if (fullPath.StartsWith(webRootPath))
            {
                var relative = fullPath.Substring(webRootPath.Length).Replace('\\', '/');
                return relative.StartsWith("/") ? relative : "/" + relative;
            }
            throw;
        }
    }
}

