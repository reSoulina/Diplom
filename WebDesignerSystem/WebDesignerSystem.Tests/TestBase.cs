using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using System.Security.Principal;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;
using WebDesignerSystem.Services;

namespace WebDesignerSystem.Tests
{
    public class TestBase
    {
        protected ApplicationDbContext CreateInMemoryDbContext(string databaseName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: databaseName)
                .Options;

            var context = new ApplicationDbContext(options);

            SeedTestData(context);

            return context;
        }

        private void SeedTestData(ApplicationDbContext context)
        {
            if (!context.Roles.Any())
            {
                context.Roles.AddRange(
                    new Role { Id = 1, Name = "Client", Description = "Клиент" },
                    new Role { Id = 2, Name = "Designer", Description = "Дизайнер" }
                );
            }

            if (!context.OrderStatuses.Any())
            {
                context.OrderStatuses.AddRange(
                    new OrderStatus { Id = 1, Name = "В очереди", Description = "Заказ ожидает обработки", Color = "#ffc107", DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
                    new OrderStatus { Id = 2, Name = "В процессе", Description = "Заказ в работе", Color = "#17a2b8", DisplayOrder = 2, CreatedAt = DateTime.UtcNow },
                    new OrderStatus { Id = 3, Name = "Готов", Description = "Заказ выполнен", Color = "#28a745", DisplayOrder = 3, CreatedAt = DateTime.UtcNow },
                    new OrderStatus { Id = 4, Name = "Отменен", Description = "Заказ отменен", Color = "#dc3545", DisplayOrder = 4, CreatedAt = DateTime.UtcNow }
                );
            }

            context.SaveChanges();
        }

        protected Mock<UserManager<User>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            var mgr = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<User>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<User>());
            return mgr;
        }

        // Добавляем метод для создания Mock UserManager без параметров (для случаев, когда он не нужен)
        protected UserManager<User> GetMockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            var mgr = new Mock<UserManager<User>>(store.Object, null, null, null, null, null, null, null, null);
            mgr.Object.UserValidators.Add(new UserValidator<User>());
            mgr.Object.PasswordValidators.Add(new PasswordValidator<User>());
            return mgr.Object;
        }

        protected ClaimsPrincipal CreateTestUser(string userId = "test-user-id", string email = "test@example.com", string role = "Client")
        {
            var identity = new GenericIdentity(email);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, email),
                new Claim(ClaimTypes.Role, role)
            };
            identity.AddClaims(claims);
            return new ClaimsPrincipal(identity);
        }

        protected User CreateTestUserEntity(string id = "test-user-id", string email = "test@example.com", string fullName = "Тестовый Пользователь", int roleId = 1)
        {
            return new User
            {
                Id = id,
                UserName = email,
                Email = email,
                FullName = fullName,
                RoleId = roleId,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        protected Product CreateTestProduct(int id = 1, string name = "Тестовый товар", decimal price = 1000, bool isService = false, int? categoryId = null)
        {
            return new Product
            {
                Id = id,
                Name = name,
                Description = "Тестовое описание",
                Price = price,
                CategoryId = categoryId,
                IsService = isService,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
        }

        protected Mock<IFileService> CreateMockFileService()
        {
            var mock = new Mock<IFileService>();
            mock.Setup(f => f.SaveImageAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
                .ReturnsAsync("/uploads/products/test-image.jpg");
            mock.Setup(f => f.GetSafeImageUrl(It.IsAny<string>()))
                .Returns<string>(url => string.IsNullOrEmpty(url) ? "/images/no-image.png" : url);
            mock.Setup(f => f.DeleteImage(It.IsAny<string>()));
            return mock;
        }

        protected TempDataDictionary CreateTempData()
        {
            var httpContext = new DefaultHttpContext();
            return new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        protected void SetupPageModelContext(PageModel pageModel, ClaimsPrincipal user, TempDataDictionary tempData)
        {
            var httpContext = new DefaultHttpContext
            {
                User = user
            };

            pageModel.PageContext = new PageContext
            {
                HttpContext = httpContext
            };
            pageModel.TempData = tempData;
            pageModel.ModelState.Clear();
        }
    }
}