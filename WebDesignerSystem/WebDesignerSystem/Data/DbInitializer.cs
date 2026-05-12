using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Создание необходимых таблиц (DesignerProfiles, ClientProfiles, CartItems)
            await context.Database.OpenConnectionAsync();
            var tablesToCreate = new Dictionary<string, string>
            {
                { "DesignerProfiles", @"
                    CREATE TABLE IF NOT EXISTS ""DesignerProfiles"" (
                        ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_DesignerProfiles"" PRIMARY KEY AUTOINCREMENT,
                        ""Name"" TEXT NOT NULL,
                        ""Position"" TEXT NULL,
                        ""Bio"" TEXT NULL,
                        ""Email"" TEXT NULL,
                        ""Phone"" TEXT NULL,
                        ""WorkingHours"" TEXT NULL,
                        ""PhotoUrl"" TEXT NULL,
                        ""PhotoPath"" TEXT NULL,
                        ""UpdatedAt"" TEXT NOT NULL
                    )" },
                { "ClientProfiles", @"
                    CREATE TABLE IF NOT EXISTS ""ClientProfiles"" (
                        ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_ClientProfiles"" PRIMARY KEY AUTOINCREMENT,
                        ""UserId"" TEXT NOT NULL,
                        ""FullName"" TEXT NULL,
                        ""Phone"" TEXT NULL,
                        ""DeliveryAddress"" TEXT NULL,
                        ""UpdatedAt"" TEXT NOT NULL
                    )" },
                { "CartItems", @"
                    CREATE TABLE IF NOT EXISTS ""CartItems"" (
                        ""Id"" INTEGER NOT NULL CONSTRAINT ""PK_CartItems"" PRIMARY KEY AUTOINCREMENT,
                        ""UserId"" TEXT NOT NULL,
                        ""ProductId"" INTEGER NOT NULL,
                        ""Quantity"" INTEGER NOT NULL,
                        ""AddedAt"" TEXT NOT NULL
                    )" }
            };
            foreach (var table in tablesToCreate)
            {
                using var cmd = context.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = table.Value;
                await cmd.ExecuteNonQueryAsync();
                Console.WriteLine($"Таблица {table.Key} проверена/создана");
            }
            await context.Database.CloseConnectionAsync();

            // 2. Создание остальных таблиц (если нет)
            context.Database.EnsureCreated();

            // 3. Добавление недостающих столбцов в существующие таблицы
            await context.Database.OpenConnectionAsync();
            // Добавление UpdatedAt в Orders
            using (var cmd = context.Database.GetDbConnection().CreateCommand())
            {
                cmd.CommandText = "SELECT COUNT(*) FROM pragma_table_info('Orders') WHERE name='UpdatedAt'";
                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                if (count == 0)
                {
                    using var alter = context.Database.GetDbConnection().CreateCommand();
                    alter.CommandText = "ALTER TABLE Orders ADD COLUMN UpdatedAt TEXT NOT NULL DEFAULT '2024-01-01 00:00:00'";
                    await alter.ExecuteNonQueryAsync();
                    Console.WriteLine("Столбец UpdatedAt добавлен в Orders");
                }
            }
            // Добавление полей в ServiceAppointments
            var columnsToAdd = new Dictionary<string, string>
            {
                { "Format", "TEXT NOT NULL DEFAULT 'online'" },
                { "ContactInfo", "TEXT NULL" },
                { "Address", "TEXT NULL" },
                { "UpdatedAt", "TEXT NOT NULL DEFAULT '2024-01-01 00:00:00'" }
            };
            foreach (var col in columnsToAdd)
            {
                using var check = context.Database.GetDbConnection().CreateCommand();
                check.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('ServiceAppointments') WHERE name='{col.Key}'";
                var exists = Convert.ToInt32(await check.ExecuteScalarAsync());
                if (exists == 0)
                {
                    using var alter = context.Database.GetDbConnection().CreateCommand();
                    alter.CommandText = $"ALTER TABLE ServiceAppointments ADD COLUMN \"{col.Key}\" {col.Value}";
                    await alter.ExecuteNonQueryAsync();
                    Console.WriteLine($"Столбец {col.Key} добавлен в ServiceAppointments");
                }
            }
            await context.Database.CloseConnectionAsync();

            // 4. Инициализация данных (роли, пользователи, статусы заказов, категории, продукты)
            if (!await roleManager.RoleExistsAsync("Client"))
                await roleManager.CreateAsync(new IdentityRole("Client"));
            if (!await roleManager.RoleExistsAsync("Designer"))
                await roleManager.CreateAsync(new IdentityRole("Designer"));

            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Id = 1, Name = "Client", Description = "Роль клиента" },
                    new Role { Id = 2, Name = "Designer", Description = "Роль дизайнера" }
                );
                await context.SaveChangesAsync();
            }

            if (!context.DesignerProfiles.Any())
            {
                context.DesignerProfiles.Add(new DesignerProfile
                {
                    Name = "Прохорова София",
                    Position = "Мега крутой дизайнер, творец и кастомщик",
                    Bio = "Создаю оригинальные и неповторимые дизайны для чего угодно.\nПродаю уже имеющиеся крутые вещи, предметы и аксессуары.\nРаботаю по вашим запросам.\nСоздам невероятное по вашему прототипу или предложу интересное решение.",
                    Email = "designer@gmail.com",
                    Phone = "+7 (999) 123-45-67",
                    WorkingHours = "Пн-Пт: 9:00 - 18:00",
                    PhotoUrl = "/images/profile_photo.jpg",
                    UpdatedAt = DateTime.UtcNow
                });
                await context.SaveChangesAsync();
            }

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
                if (result1.Succeeded) await userManager.AddToRoleAsync(designer, "Designer");

                var client = new User
                {
                    UserName = "client@example.com",
                    Email = "client@example.com",
                    FullName = "Алексей Клиентов",
                    RoleId = 1,
                    EmailConfirmed = true
                };
                var result2 = await userManager.CreateAsync(client, "Client123!");
                if (result2.Succeeded) await userManager.AddToRoleAsync(client, "Client");
            }

            if (!context.OrderStatuses.Any())
            {
                context.OrderStatuses.AddRange(
                    new OrderStatus { Id = 1, Name = "В очереди", Color = "#ffc107", Description = "Заказ ожидает обработки" },
                    new OrderStatus { Id = 2, Name = "В процессе", Color = "#17a2b8", Description = "Заказ в работе" },
                    new OrderStatus { Id = 3, Name = "Готов", Color = "#28a745", Description = "Заказ выполнен" },
                    new OrderStatus { Id = 4, Name = "Отменен", Color = "#dc3545", Description = "Заказ отменен" }
                );
                await context.SaveChangesAsync();
            }

            if (!context.Categories.Any())
            {
                context.Categories.AddRange(
                    new Category { Id = 1, Name = "Шаблоны сайтов", Description = "Готовые шаблоны для различных типов сайтов" },
                    new Category { Id = 2, Name = "Дизайн логотипов", Description = "Разработка уникальных логотипов" },
                    new Category { Id = 3, Name = "Консультации", Description = "Профессиональные консультации по веб-дизайну" },
                    new Category { Id = 4, Name = "Разработка под ключ", Description = "Полный цикл разработки сайтов" }
                );
                await context.SaveChangesAsync();
            }

            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product { Name = "Шаблон интернет-магазина", Description = "Готовый адаптивный шаблон для интернет-магазина с корзиной и фильтрами", Price = 5000, CategoryId = 1, IsService = false, IsActive = true, ImageUrl = "https://via.placeholder.com/400x300/007bff/ffffff?text=Магазин", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Логотип для кафе", Description = "Уникальный дизайн логотипа для кофейни или ресторана", Price = 3000, CategoryId = 2, IsService = false, IsActive = true, ImageUrl = "https://via.placeholder.com/400x300/28a745/ffffff?text=Логотип", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Консультация по веб-дизайну", Description = "Индивидуальная консультация 1 час по вопросам веб-дизайна", Price = 1500, CategoryId = 3, IsService = true, IsActive = true, ImageUrl = "https://via.placeholder.com/400x300/17a2b8/ffffff?text=Консультация", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Корпоративный сайт", Description = "Разработка корпоративного сайта под ключ", Price = 25000, CategoryId = 1, IsService = true, IsActive = true, ImageUrl = "https://via.placeholder.com/400x300/6f42c1/ffffff?text=Корпоративный", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Фирменный бланк", Description = "Дизайн фирменного бланка для документов", Price = 2000, CategoryId = 2, IsService = false, IsActive = true, ImageUrl = "https://via.placeholder.com/400x300/fd7e14/ffffff?text=Бланк", CreatedAt = DateTime.UtcNow },
                    new Product { Name = "Лендинг страница", Description = "Создание одностраничного сайта для продвижения услуги", Price = 15000, CategoryId = 1, IsService = true, IsActive = true, ImageUrl = "https://via.placeholder.com/400x300/dc3545/ffffff?text=Лендинг", CreatedAt = DateTime.UtcNow }
                );
                await context.SaveChangesAsync();
            }

            Console.WriteLine("✅ База данных успешно инициализирована!");
        }
    }
}