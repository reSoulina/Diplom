// Data/DatabaseInitializer.cs
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Models.Entities;

namespace WebDesignerSystem.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // Создаем БД если ее нет
            context.Database.EnsureCreated();

            // Проверяем, есть ли уже данные
            if (context.Users.Any())
            {
                return;
            }

            // Создаем начальные данные
            var roles = new[]
            {
                new Role { Id = 1, Name = "Client" },
                new Role { Id = 2, Name = "Designer" }
            };

            context.Roles.AddRange(roles);

            var categories = new[]
            {
                new Category { Id = 1, Name = "Шаблоны сайтов", Description = "Готовые шаблоны" },
                new Category { Id = 2, Name = "Логотипы", Description = "Дизайн логотипов" },
                new Category { Id = 3, Name = "Консультации", Description = "Профессиональные консультации" }
            };

            context.Categories.AddRange(categories);

            var statuses = new[]
            {
                new OrderStatus { Id = 1, Name = "В очереди", Color = "#ffc107" },
                new OrderStatus { Id = 2, Name = "В процессе", Color = "#17a2b8" },
                new OrderStatus { Id = 3, Name = "Готов", Color = "#28a745" },
                new OrderStatus { Id = 4, Name = "Отменен", Color = "#dc3545" }
            };

            context.OrderStatuses.AddRange(statuses);

            context.SaveChanges();
        }
    }
}