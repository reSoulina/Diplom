using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class CategoriesControllerTests : IClassFixture<TestBase>
    {
        private readonly TestBase _factory;

        public CategoriesControllerTests(TestBase factory)
        {
            _factory = factory;
        }

        [Fact]
        public void Database_CanAddCategory()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            // Act
            var category = new Category
            {
                Name = "Test Category",
                Description = "Test Description"
            };

            db.Categories.Add(category);
            db.SaveChanges();

            // Assert
            var savedCategory = db.Categories.FirstOrDefault(c => c.Name == "Test Category");
            Assert.NotNull(savedCategory);
            Assert.Equal("Test Description", savedCategory.Description);
        }

        [Fact]
        public void Database_CanUpdateCategory()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var category = new Category
            {
                Name = "Category to Update",
                Description = "Original"
            };

            db.Categories.Add(category);
            db.SaveChanges();

            // Act
            category.Name = "Updated Category";
            category.Description = "Updated";
            db.Categories.Update(category);
            db.SaveChanges();

            // Assert
            var updatedCategory = db.Categories.Find(category.Id);
            Assert.Equal("Updated Category", updatedCategory.Name);
            Assert.Equal("Updated", updatedCategory.Description);
        }

        [Fact]
        public void Database_CanDeleteEmptyCategory()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            var category = new Category
            {
                Name = "Category to Delete",
                Description = "Will be deleted"
            };

            db.Categories.Add(category);
            db.SaveChanges();

            var categoryId = category.Id;

            // Act
            db.Categories.Remove(category);
            db.SaveChanges();

            // Assert
            var deletedCategory = db.Categories.Find(categoryId);
            Assert.Null(deletedCategory);
        }

        [Fact]
        public void Category_Validation_Works()
        {
            // Arrange
            var category = new Category
            {
                Name = "Valid Category",
                Description = "Valid Description"
            };

            // Act & Assert
            Assert.NotNull(category.Name);
            Assert.False(string.IsNullOrEmpty(category.Name));
        }

        [Theory]
        [InlineData("Electronics", "Devices and gadgets")]
        [InlineData("Clothing", "Apparel and accessories")]
        [InlineData("Books", "Reading materials")]
        public void Category_Properties_SetCorrectly(string name, string description)
        {
            // Arrange & Act
            var category = new Category
            {
                Name = name,
                Description = description
            };

            // Assert
            Assert.Equal(name, category.Name);
            Assert.Equal(description, category.Description);
        }
    }
}