using Microsoft.EntityFrameworkCore;
using Web_Ban_Laptop.Models;
using Web_Ban_Laptop.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext
var connectionString = builder.Configuration.GetConnectionString("MyCnn") 
    ?? "server =DESKTOP-FHDMQHT; database = LaptopStoreDB_Final; uid=sa;pwd=123;Integrated security = True; TrustServerCertificate=True;";
builder.Services.AddDbContext<LaptopStoreDbFinalContext>(options =>
    options.UseSqlServer(connectionString));

// Configure Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register Services
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<CartService>();
builder.Services.AddScoped<ProductImageSeeder>();

// Configure Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Seed ProductImages (chỉ chạy trong Development)
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var seeder = new Web_Ban_Laptop.Services.ProductImageSeeder(
                services.GetRequiredService<LaptopStoreDbFinalContext>(),
                services.GetRequiredService<IWebHostEnvironment>(),
                services.GetRequiredService<ILogger<Web_Ban_Laptop.Services.ProductImageSeeder>>(),
                services.GetRequiredService<IConfiguration>()
            );
            
            // Seed ProductImages (mặc định true trong Development)
            var shouldSeed = builder.Configuration.GetValue<bool>("SeedProductImages", true);
            if (shouldSeed)
            {
                await seeder.SeedProductImagesAsync();
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "❌ Lỗi khi seed ProductImages");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
