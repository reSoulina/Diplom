using System.ComponentModel.DataAnnotations;
using WebDesignerSystem.Models.ViewModels;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class ViewModelValidationTests
    {
        private List<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        [Fact]
        public void RegisterViewModel_ValidData_PassesValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = "Тестовый Пользователь",
                Email = "test@example.com",
                Role = "Client",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            // Act
            var errors = ValidateModel(model);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void RegisterViewModel_MissingFullName_FailsValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = null,
                Email = "test@example.com",
                Role = "Client",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            // Act
            var errors = ValidateModel(model);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("FullName"));
        }

        [Fact]
        public void RegisterViewModel_InvalidEmail_FailsValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = "Тестовый Пользователь",
                Email = "invalid-email",
                Role = "Client",
                Password = "Password123",
                ConfirmPassword = "Password123"
            };

            // Act
            var errors = ValidateModel(model);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("Email"));
        }

        [Fact]
        public void RegisterViewModel_PasswordTooShort_FailsValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = "Тестовый Пользователь",
                Email = "test@example.com",
                Role = "Client",
                Password = "123",
                ConfirmPassword = "123"
            };

            // Act
            var errors = ValidateModel(model);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("Password"));
        }

        [Fact]
        public void RegisterViewModel_PasswordsDoNotMatch_FailsValidation()
        {
            // Arrange
            var model = new RegisterViewModel
            {
                FullName = "Тестовый Пользователь",
                Email = "test@example.com",
                Role = "Client",
                Password = "Password123",
                ConfirmPassword = "Password456"
            };

            // Act
            var errors = ValidateModel(model);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("ConfirmPassword"));
        }

        [Fact]
        public void LoginViewModel_ValidData_PassesValidation()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = "Password123",
                RememberMe = true
            };

            // Act
            var errors = ValidateModel(model);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void LoginViewModel_MissingEmail_FailsValidation()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = null,
                Password = "Password123"
            };

            // Act
            var errors = ValidateModel(model);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("Email"));
        }

        [Fact]
        public void LoginViewModel_MissingPassword_FailsValidation()
        {
            // Arrange
            var model = new LoginViewModel
            {
                Email = "test@example.com",
                Password = null
            };

            // Act
            var errors = ValidateModel(model);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("Password"));
        }

        [Fact]
        public void ProductViewModel_ValidData_PassesValidation()
        {
            // Arrange
            var model = new ProductViewModel
            {
                Name = "Тестовый товар",
                Description = "Описание",
                Price = 1000,
                CategoryId = 1,
                ProductType = "product",
                IsActive = true
            };

            // Act
            var errors = ValidateModel(model);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ProductViewModel_MissingName_FailsValidation()
        {
            // Arrange
            var model = new ProductViewModel
            {
                Name = null,
                Price = 1000,
                ProductType = "product"
            };

            // Act
            var errors = ValidateModel(model);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("Name"));
        }

        [Fact]
        public void ProductViewModel_PriceZero_FailsValidation()
        {
            // Arrange
            var model = new ProductViewModel
            {
                Name = "Тестовый товар",
                Price = 0,
                ProductType = "product"
            };

            // Act
            var errors = ValidateModel(model);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("Price"));
        }

        [Fact]
        public void ProductViewModel_NameTooLong_FailsValidation()
        {
            // Arrange
            var model = new ProductViewModel
            {
                Name = new string('A', 101),
                Price = 1000,
                ProductType = "product"
            };

            // Act
            var errors = ValidateModel(model);

            // Assert
            Assert.Contains(errors, e => e.MemberNames.Contains("Name"));
        }
    }
}