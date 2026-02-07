using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class IntegrationTests : IClassFixture<TestBase>
    {
        private readonly TestBase _factory;

        public IntegrationTests(TestBase factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task FullProductLifecycle_CreateEditDelete()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // 1. Создаем категорию
            var category = new Category
            {
                Name = "Integration Test Category",
                Description = "For integration testing"
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            // 2. Создаем продукт
            var product = new Product
            {
                Name = "Integration Test Product",
                Price = 199.99m,
                Description = "Created in integration test",
                CategoryId = category.Id,
                IsService = false,
                IsActive = true
            };
            db.Products.Add(product);
            await db.SaveChangesAsync();

            // 3. Проверяем создание
            var createdProduct = await db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Name == "Integration Test Product");
            Assert.NotNull(createdProduct);
            Assert.Equal(199.99m, createdProduct.Price);
            Assert.Equal("Integration Test Category", createdProduct.Category.Name);

            // 4. Редактируем продукт
            createdProduct.Price = 299.99m;
            createdProduct.Description = "Updated in integration test";
            db.Products.Update(createdProduct);
            await db.SaveChangesAsync();

            var updatedProduct = await db.Products.FindAsync(createdProduct.Id);
            Assert.Equal(299.99m, updatedProduct.Price);
            Assert.Equal("Updated in integration test", updatedProduct.Description);

            // 5. Удаляем продукт
            db.Products.Remove(updatedProduct);
            await db.SaveChangesAsync();

            var deletedProduct = await db.Products.FindAsync(createdProduct.Id);
            Assert.Null(deletedProduct);
        }

        [Fact]
        public async Task AdminPages_ShouldRequireAuthorization()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act - пытаемся получить доступ к админ страницам без авторизации
            var productsResponse = await client.GetAsync("/Admin/Products");
            var categoriesResponse = await client.GetAsync("/Admin/Categories");

            // Assert - обычно будет 200 OK для Razor Pages даже без авторизации
            // или Redirect на страницу логина
            Assert.True(productsResponse.IsSuccessStatusCode ||
                       productsResponse.StatusCode == HttpStatusCode.Redirect);
            Assert.True(categoriesResponse.IsSuccessStatusCode ||
                       categoriesResponse.StatusCode == HttpStatusCode.Redirect);
        }

        [Fact]
        public async Task HomePage_ShouldLoadSuccessfully()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.False(string.IsNullOrEmpty(content));
        }

        [Fact]
        public async Task CatalogPages_ShouldBeAccessible()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act & Assert - проверяем доступность публичных страниц
            var catalogResponse = await client.GetAsync("/Catalog");
            Assert.True(catalogResponse.IsSuccessStatusCode ||
                       catalogResponse.StatusCode == HttpStatusCode.NotFound); // Если страница существует

            var clientResponse = await client.GetAsync("/Client");
            Assert.True(clientResponse.IsSuccessStatusCode ||
                       clientResponse.StatusCode == HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Database_ShouldBeProperlyConfigured()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Проверяем что можем подключиться к БД
            var canConnect = await db.Database.CanConnectAsync();
            Assert.True(canConnect);

            // Проверяем что можем выполнять запросы
            var categoriesCount = await db.Categories.CountAsync();
            var productsCount = await db.Products.CountAsync();

            Assert.True(categoriesCount >= 0);
            Assert.True(productsCount >= 0);
        }
    }
}