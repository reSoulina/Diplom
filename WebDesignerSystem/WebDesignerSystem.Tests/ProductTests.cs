using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class ProductTests : IClassFixture<TestBase>
    {
        private readonly TestBase _factory;

        public ProductTests(TestBase factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ProductsPage_LoadsSuccessfully()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/Admin/Products");

            // Assert
            // Может быть OK или Redirect если требуется авторизация
            Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Redirect);
        }

        [Fact]
        public void CreateProduct_InMemoryDatabase_Works()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Act
            var product = new Product
            {
                Name = "Test Product",
                Price = 100.50m,
                Description = "Test Description",
                IsActive = true,
                IsService = false
            };

            db.Products.Add(product);
            db.SaveChanges();

            // Assert
            var savedProduct = db.Products.FirstOrDefault(p => p.Name == "Test Product");
            Assert.NotNull(savedProduct);
            Assert.Equal(100.50m, savedProduct.Price);
        }
    }
}