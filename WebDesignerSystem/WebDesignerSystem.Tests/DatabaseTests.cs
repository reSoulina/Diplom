using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class DatabaseTests : IClassFixture<TestBase>
    {
        private readonly TestBase _factory;

        public DatabaseTests(TestBase factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Database_CanCreateAndSaveProduct()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var category = new Category
            {
                Name = "Test Category",
                Description = "For testing products"
            };
            db.Categories.Add(category);
            await db.SaveChangesAsync();

            // Act
            var product = new Product
            {
                Name = "Database Test Product",
                Price = 199.99m,
                Description = "Product for database testing",
                CategoryId = category.Id,
                IsService = false,
                IsActive = true
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            // Assert
            var savedProduct = await db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Name == "Database Test Product");

            Assert.NotNull(savedProduct);
            Assert.Equal(199.99m, savedProduct.Price);
            Assert.Equal("Test Category", savedProduct.Category.Name);
        }

        [Fact]
        public async Task Database_CanUpdateProduct()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var product = new Product
            {
                Name = "Product to Update",
                Price = 100m,
                Description = "Original",
                IsService = false,
                IsActive = true
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            // Act
            product.Price = 150m;
            product.Description = "Updated";
            db.Products.Update(product);
            await db.SaveChangesAsync();

            // Assert
            var updatedProduct = await db.Products.FindAsync(product.Id);
            Assert.NotNull(updatedProduct);
            Assert.Equal(150m, updatedProduct.Price);
            Assert.Equal("Updated", updatedProduct.Description);
        }

        [Fact]
        public async Task Database_CanDeleteProduct()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var product = new Product
            {
                Name = "Product to Delete",
                Price = 100m,
                Description = "Will be deleted",
                IsService = false,
                IsActive = true
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            var productId = product.Id;

            // Act
            db.Products.Remove(product);
            await db.SaveChangesAsync();

            // Assert
            var deletedProduct = await db.Products.FindAsync(productId);
            Assert.Null(deletedProduct);
        }
    }
}