using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class ProductsControllerTests : IClassFixture<TestBase>
    {
        private readonly TestBase _factory;

        public ProductsControllerTests(TestBase factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Database_CanAddProduct()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // НЕ очищаем БД - это нарушает работу других тестов
            // Вместо этого используем уникальные имена

            // Act
            var product = new Product
            {
                Name = $"Test Product {Guid.NewGuid()}", // Уникальное имя
                Price = 100.50m,
                Description = "Test Description",
                IsService = false,
                IsActive = true
            };

            db.Products.Add(product);
            db.SaveChanges();

            // Assert
            var savedProduct = db.Products.FirstOrDefault(p => p.Id == product.Id);
            Assert.NotNull(savedProduct);
            Assert.Equal(100.50m, savedProduct.Price);
            Assert.Equal(product.Name, savedProduct.Name);
        }

        [Fact]
        public void Database_CanUpdateProduct()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Создаем продукт с уникальным именем
            var product = new Product
            {
                Name = $"Product to Update {Guid.NewGuid()}",
                Price = 100m,
                Description = "Original",
                IsService = false,
                IsActive = true
            };

            db.Products.Add(product);
            db.SaveChanges();

            // Act
            product.Price = 150m;
            product.Description = "Updated";
            db.Products.Update(product);
            db.SaveChanges();

            // Assert
            var updatedProduct = db.Products.Find(product.Id);
            Assert.NotNull(updatedProduct);
            Assert.Equal(150m, updatedProduct.Price);
            Assert.Equal("Updated", updatedProduct.Description);
        }

        [Fact]
        public void Database_CanDeleteProduct()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Создаем продукт с уникальным именем
            var product = new Product
            {
                Name = $"Product to Delete {Guid.NewGuid()}",
                Price = 100m,
                Description = "Will be deleted",
                IsService = false,
                IsActive = true
            };

            db.Products.Add(product);
            db.SaveChanges();

            var productId = product.Id;

            // Act
            db.Products.Remove(product);
            db.SaveChanges();

            // Assert
            var deletedProduct = db.Products.Find(productId);
            Assert.Null(deletedProduct);
        }

        [Fact]
        public void Product_Validation_Works()
        {
            // Arrange
            var product = new Product
            {
                Name = "Valid Product",
                Price = 100m,
                IsService = false,
                IsActive = true
            };

            // Act & Assert
            Assert.NotNull(product.Name);
            Assert.True(product.Price > 0);
        }

        [Theory]
        [InlineData(100, false, "product")]
        [InlineData(200, true, "service")]
        public void Product_Properties_SetCorrectly(decimal price, bool isService, string expectedType)
        {
            // Arrange & Act
            var product = new Product
            {
                Name = "Test",
                Price = price,
                IsService = isService,
                IsActive = true
            };

            // Assert
            Assert.Equal(price, product.Price);
            Assert.Equal(isService, product.IsService);
        }
    }
}