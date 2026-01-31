using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                // Проверяем, есть ли уже данные
                if (context.Users.Any())
                {
                    return; // БД уже инициализирована
                }

                // Создаем роли Identity
                var roles = new[] { "Client", "Designer" };
                foreach (var roleName in roles)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Создаем тестового дизайнера
                var designerEmail = "designer@example.com";
                if (await userManager.FindByEmailAsync(designerEmail) == null)
                {
                    var designer = new User
                    {
                        UserName = designerEmail,
                        Email = designerEmail,
                        FullName = "Иван Дизайнеров",
                        RoleId = 2, // Designer role ID из таблицы Roles
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(designer, "Designer123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(designer, "Designer");
                    }
                }

                // Создаем тестового клиента
                var clientEmail = "client@example.com";
                if (await userManager.FindByEmailAsync(clientEmail) == null)
                {
                    var client = new User
                    {
                        UserName = clientEmail,
                        Email = clientEmail,
                        FullName = "Алексей Клиентов",
                        RoleId = 1, // Client role ID из таблицы Roles
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(client, "Client123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(client, "Client");
                    }
                }

                // Создаем тестовые продукты
                if (!context.Products.Any())
                {
                    context.Products.AddRange(
                        new Product
                        {
                            Name = "Шаблон интернет-магазина",
                            Description = "Готовый шаблон для интернет-магазина",
                            Price = 5000,
                            CategoryId = 1,
                            IsService = false,
                            IsActive = true
                        },
                        new Product
                        {
                            Name = "Логотип для кафе",
                            Description = "Уникальный логотип для кофейни",
                            Price = 3000,
                            CategoryId = 2,
                            IsService = false,
                            IsActive = true
                        },
                        new Product
                        {
                            Name = "Консультация по веб-дизайну",
                            Description = "Индивидуальная консультация 1 час",
                            Price = 1500,
                            CategoryId = 3,
                            IsService = true,
                            IsActive = true
                        }
                    );
                }

                await context.SaveChangesAsync();
            }
        }
    }
}