using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;
using WebDesignerSystem.Services;

var builder = WebApplication.CreateBuilder(args);

// Настройка сервисов
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Создаём папки для загрузок и фото
var uploadsPath = Path.Combine(app.Environment.WebRootPath, "uploads", "products");
if (!Directory.Exists(uploadsPath)) Directory.CreateDirectory(uploadsPath);

var designerPhotosPath = Path.Combine(app.Environment.WebRootPath, "images", "designer");
if (!Directory.Exists(designerPhotosPath)) Directory.CreateDirectory(designerPhotosPath);

// Инициализация базы данных (вынесено в отдельный класс)
using (var scope = app.Services.CreateScope())
{
    await DbInitializer.InitializeAsync(scope.ServiceProvider);
}

// Конфигурация HTTP pipeline
if (app.Environment.IsDevelopment()) app.UseDeveloperExceptionPage();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Debug-эндпоинт
app.MapGet("/debug/db", async (ApplicationDbContext db, IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("DefaultConnection");
    var dbPath = connectionString?.Replace("Data Source=", "").Trim() ?? "не указан";
    var fullPath = Path.GetFullPath(dbPath);
    var exists = File.Exists(fullPath);

    int designerProfilesCount = 0, clientProfilesCount = 0, cartItemsCount = 0;
    try { designerProfilesCount = await db.DesignerProfiles.CountAsync(); } catch { designerProfilesCount = -1; }
    try { clientProfilesCount = await db.ClientProfiles.CountAsync(); } catch { clientProfilesCount = -1; }
    try { cartItemsCount = await db.CartItems.CountAsync(); } catch { cartItemsCount = -1; }

    return Results.Ok(new
    {
        ConnectionString = connectionString,
        DbPath = dbPath,
        FullPath = fullPath,
        Exists = exists,
        FileSize = exists ? new FileInfo(fullPath).Length : 0,
        CurrentDirectory = Directory.GetCurrentDirectory(),
        UsersCount = await db.Users.CountAsync(),
        RolesCount = await db.Roles.CountAsync(),
        DesignerProfilesCount = designerProfilesCount,
        ClientProfilesCount = clientProfilesCount,
        CartItemsCount = cartItemsCount,
        ProductsCount = await db.Products.CountAsync(),
        CategoriesCount = await db.Categories.CountAsync(),
        StatusesCount = await db.OrderStatuses.CountAsync()
    });
});

app.Run();

public partial class Program { }