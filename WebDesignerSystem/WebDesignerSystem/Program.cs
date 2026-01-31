using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;

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
    options.Password.RequireNonAlphanumeric = false; // Можно включить если нужно
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Путь к БД для отладки
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection string: {connectionString}");

// Создаем БД при первом запуске
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Получаем путь к файлу БД
        var dbPath = connectionString.Replace("Data Source=", "").Trim();
        Console.WriteLine($"DB Path: {dbPath}");

        // Создаем папку если нужно
        var directory = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            Console.WriteLine($"Created directory: {directory}");
        }

        // Создаем БД
        var created = context.Database.EnsureCreated();
        Console.WriteLine($"Database created: {created}");

        // Создаем роли Identity
        if (!await roleManager.RoleExistsAsync("Client"))
        {
            await roleManager.CreateAsync(new IdentityRole("Client"));
            Console.WriteLine("Created Identity role: Client");
        }

        if (!await roleManager.RoleExistsAsync("Designer"))
        {
            await roleManager.CreateAsync(new IdentityRole("Designer"));
            Console.WriteLine("Created Identity role: Designer");
        }

        // Создаем ваши кастомные роли (ОБНОВЛЕНО: добавлены Description)
        if (!context.Roles.Any())
        {
            context.Roles.AddRange(
                new Role
                {
                    Id = 1,
                    Name = "Client",
                    Description = "Роль клиента" // Добавлено
                },
                new Role
                {
                    Id = 2,
                    Name = "Designer",
                    Description = "Роль дизайнера" // Добавлено
                }
            );
            await context.SaveChangesAsync();
            Console.WriteLine("Created custom roles with descriptions");
        }

        // Создаем пользователей
        if (!context.Users.Any())
        {
            var designer = new User
            {
                UserName = "designer@example.com",
                Email = "designer@example.com",
                FullName = "Иван Дизайнеров",
                RoleId = 2,
                EmailConfirmed = true
            };

            var result1 = await userManager.CreateAsync(designer, "Designer123!");
            if (result1.Succeeded)
            {
                await userManager.AddToRoleAsync(designer, "Designer");
                Console.WriteLine("Created designer user");
            }
            else
            {
                Console.WriteLine($"Error creating designer: {string.Join(", ", result1.Errors.Select(e => e.Description))}");
            }

            var client = new User
            {
                UserName = "client@example.com",
                Email = "client@example.com",
                FullName = "Алексей Клиентов",
                RoleId = 1,
                EmailConfirmed = true
            };

            var result2 = await userManager.CreateAsync(client, "Client123!");
            if (result2.Succeeded)
            {
                await userManager.AddToRoleAsync(client, "Client");
                Console.WriteLine("Created client user");
            }
            else
            {
                Console.WriteLine($"Error creating client: {string.Join(", ", result2.Errors.Select(e => e.Description))}");
            }
        }

        // Создаем статусы заказов (ОБНОВЛЕНО: добавлены Description)
        if (!context.OrderStatuses.Any())
        {
            context.OrderStatuses.AddRange(
                new OrderStatus
                {
                    Id = 1,
                    Name = "В очереди",
                    Color = "#ffc107",
                    Description = "Заказ ожидает обработки" // Добавлено
                },
                new OrderStatus
                {
                    Id = 2,
                    Name = "В процессе",
                    Color = "#17a2b8",
                    Description = "Заказ в работе" // Добавлено
                },
                new OrderStatus
                {
                    Id = 3,
                    Name = "Готов",
                    Color = "#28a745",
                    Description = "Заказ выполнен" // Добавлено
                },
                new OrderStatus
                {
                    Id = 4,
                    Name = "Отменен",
                    Color = "#dc3545",
                    Description = "Заказ отменен" // Добавлено
                }
            );
            await context.SaveChangesAsync();
            Console.WriteLine("Created order statuses");
        }

        // Создаем категории (ОБНОВЛЕНО: добавлены Description)
        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category
                {
                    Id = 1,
                    Name = "Шаблоны сайтов",
                    Description = "Готовые шаблоны для сайтов" // Добавлено
                },
                new Category
                {
                    Id = 2,
                    Name = "Логотипы",
                    Description = "Дизайн логотипов" // Добавлено
                },
                new Category
                {
                    Id = 3,
                    Name = "Консультации",
                    Description = "Профессиональные консультации" // Добавлено
                }
            );
            await context.SaveChangesAsync();
            Console.WriteLine("Created categories");
        }

        // Создаем продукты
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product
                {
                    Name = "Шаблон интернет-магазина",
                    Description = "Готовый шаблон",
                    Price = 5000,
                    CategoryId = 1,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Логотип для кафе",
                    Description = "Уникальный дизайн",
                    Price = 3000,
                    CategoryId = 2,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                },
                new Product
                {
                    Name = "Консультация по веб-дизайну",
                    Description = "1 час консультации",
                    Price = 1500,
                    CategoryId = 3,
                    IsService = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
            await context.SaveChangesAsync();
            Console.WriteLine("Created products");
        }

        Console.WriteLine("✅ База данных успешно инициализирована!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Ошибка при создании БД: {ex.Message}");
        if (ex.InnerException != null)
        {
            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
        }
    }
}

// Конфигурация pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Эндпоинт для проверки БД
app.MapGet("/debug/db", async (ApplicationDbContext db, IConfiguration config) =>
{
    var connectionString = config.GetConnectionString("DefaultConnection");
    var dbPath = connectionString?.Replace("Data Source=", "").Trim() ?? "не указан";
    var fullPath = Path.GetFullPath(dbPath);
    var exists = File.Exists(fullPath);

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
        ProductsCount = await db.Products.CountAsync(),
        CategoriesCount = await db.Categories.CountAsync(),
        StatusesCount = await db.OrderStatuses.CountAsync()
    });
});

app.Run();