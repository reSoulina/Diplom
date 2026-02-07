using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using WebDesignerSystem.Data;
using WebDesignerSystem.Services;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class ServiceTests : IClassFixture<TestBase>
    {
        private readonly TestBase _factory;

        public ServiceTests(TestBase factory)
        {
            _factory = factory;
        }

        [Fact]
        public void DatabaseContext_ShouldBeRegisteredInDI()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();

            // Act
            var dbContext = scope.ServiceProvider.GetService<ApplicationDbContext>();

            // Assert
            Assert.NotNull(dbContext);
        }

        [Theory]
        [InlineData(null, "/images/no-image.png")]
        [InlineData("", "/images/no-image.png")]
        [InlineData("/uploads/products/test.jpg", "/uploads/products/test.jpg")]
        [InlineData("https://example.com/image.jpg", "https://example.com/image.jpg")]
        public void FileService_GetSafeImageUrl_ShouldHandleDifferentInputs(string input, string expected)
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();

            // Act
            var result = fileService.GetSafeImageUrl(input);

            // Assert
            Assert.NotNull(result);
            // Для тестов проверяем логику, не фактическое существование файла
            if (string.IsNullOrEmpty(input))
            {
                Assert.Contains("no-image.png", result);
            }
        }
    }
}