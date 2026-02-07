using System.ComponentModel.DataAnnotations;
using WebDesignerSystem.Models.Entities;
using WebDesignerSystem.Models.ViewModels;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class ModelTests
    {
        [Fact]
        public void ProductViewModel_ValidData_ShouldPassValidation()
        {
            // Arrange
            var model = new ProductViewModel
            {
                Name = "Test Product",
                Price = 99.99m,
                ProductType = "product",
                Description = "Test Description",
                IsActive = true
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true);

            // Assert
            Assert.True(isValid);
            Assert.Empty(validationResults);
        }

        [Fact]
        public void ProductViewModel_InvalidPrice_ShouldFailValidation()
        {
            // Arrange
            var model = new ProductViewModel
            {
                Name = "Test Product",
                Price = -10m, // Отрицательная цена
                ProductType = "product"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.ErrorMessage.Contains("Цена должна быть больше 0"));
        }

        [Fact]
        public void ProductViewModel_MissingName_ShouldFailValidation()
        {
            // Arrange
            var model = new ProductViewModel
            {
                Name = "", // Пустое имя
                Price = 100m,
                ProductType = "product"
            };

            // Act
            var validationResults = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(model, new ValidationContext(model), validationResults, true);

            // Assert
            Assert.False(isValid);
            Assert.Contains(validationResults, r => r.ErrorMessage.Contains("Название обязательно"));
        }

        [Fact]
        public void Product_Entity_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var product = new Product
            {
                Id = 1,
                Name = "Test Product",
                Price = 100.50m,
                Description = "Description",
                IsService = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Assert
            Assert.Equal(1, product.Id);
            Assert.Equal("Test Product", product.Name);
            Assert.Equal(100.50m, product.Price);
            Assert.False(product.IsService);
            Assert.True(product.IsActive);
        }

        [Fact]
        public void Category_Entity_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var category = new Category
            {
                Id = 1,
                Name = "Test Category",
                Description = "Test Description"
            };

            // Assert
            Assert.Equal(1, category.Id);
            Assert.Equal("Test Category", category.Name);
            Assert.Equal("Test Description", category.Description);
        }

        [Theory]
        [InlineData("product", false)]
        [InlineData("service", true)]
        public void ProductViewModel_IsService_ShouldReturnCorrectValue(string productType, bool expectedIsService)
        {
            // Arrange
            var model = new ProductViewModel
            {
                ProductType = productType
            };

            // Act & Assert
            Assert.Equal(expectedIsService, model.IsService);
        }
    }
}