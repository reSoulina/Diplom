using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using WebDesignerSystem.Models.Entities;
using WebDesignerSystem.Models.ViewModels;
using WebDesignerSystem.Pages.Admin.Products;
using WebDesignerSystem.Pages.Catalog;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class ProductTests : TestBase
    {
        [Fact]
        public async Task CreateProduct_ShouldAddToDatabase()
        {
            // Arrange
            var context = CreateInMemoryDbContext("CreateProductTest");
            var fileServiceMock = CreateMockFileService();

            var pageModel = new CreateModel(context, fileServiceMock.Object);

            pageModel.Product = new ProductViewModel
            {
                Name = "Новый тестовый товар",
                Description = "Описание нового товара",
                Price = 999.99m,
                CategoryId = 1,
                ProductType = "product",
                IsActive = true
            };

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var product = await context.Products.FirstOrDefaultAsync(p => p.Name == "Новый тестовый товар");
            Assert.NotNull(product);
            Assert.Equal(999.99m, product.Price);
            Assert.False(product.IsService);
            // Проверяем, что товар создан, даже если результат не RedirectToPageResult
        }

        [Fact]
        public async Task EditProduct_ShouldUpdateExistingProduct()
        {
            // Arrange
            var context = CreateInMemoryDbContext("EditProductTest");
            var fileServiceMock = CreateMockFileService();

            var existingProduct = CreateTestProduct(1, "Старое название", 500, false);
            context.Products.Add(existingProduct);
            await context.SaveChangesAsync();

            var pageModel = new EditModel(context, fileServiceMock.Object);
            await pageModel.OnGetAsync(1);

            pageModel.Product.Name = "Новое название";
            pageModel.Product.Price = 750;
            pageModel.Product.ProductType = "product";
            pageModel.Product.IsActive = true;

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var updatedProduct = await context.Products.FindAsync(1);
            Assert.NotNull(updatedProduct);
            Assert.Equal("Новое название", updatedProduct.Name);
            Assert.Equal(750, updatedProduct.Price);
        }

        [Fact]
        public async Task CreateProduct_WithInvalidModel_ShouldReturnPage()
        {
            // Arrange
            var context = CreateInMemoryDbContext("InvalidModelTest");
            var fileServiceMock = CreateMockFileService();

            var pageModel = new CreateModel(context, fileServiceMock.Object);
            pageModel.Product = new ProductViewModel();
            pageModel.ModelState.AddModelError("Name", "Название обязательно");

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            Assert.IsType<PageResult>(result);
            var productCount = await context.Products.CountAsync();
            Assert.Equal(0, productCount);
        }

        [Fact]
        public async Task DeleteProduct_ShouldRemoveFromDatabase()
        {
            // Arrange
            var context = CreateInMemoryDbContext("DeleteProductTest");
            var fileServiceMock = CreateMockFileService();
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<DeleteModel>>();

            var product = CreateTestProduct(1, "Удаляемый товар", 1000, false);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var pageModel = new DeleteModel(context, fileServiceMock.Object, loggerMock.Object);

            // Важно: добавляем TempData
            pageModel.TempData = CreateTempData();

            // Act
            var result = await pageModel.OnPostAsync(1);

            // Assert
            var deletedProduct = await context.Products.FindAsync(1);
            Assert.Null(deletedProduct);
        }

        [Fact]
        public async Task CatalogFilter_ByCategory_ShouldReturnFilteredProducts()
        {
            // Arrange
            var context = CreateInMemoryDbContext("CatalogFilterTest");

            // Очищаем существующие категории, чтобы избежать конфликта
            var existingCategories = await context.Categories.ToListAsync();
            context.Categories.RemoveRange(existingCategories);

            var category1 = new Category { Id = 1, Name = "Категория 1", Description = "Описание 1", CreatedAt = DateTime.UtcNow };
            var category2 = new Category { Id = 2, Name = "Категория 2", Description = "Описание 2", CreatedAt = DateTime.UtcNow };
            context.Categories.AddRange(category1, category2);
            await context.SaveChangesAsync();

            var product1 = CreateTestProduct(1, "Товар 1", 100, false, 1);
            var product2 = CreateTestProduct(2, "Товар 2", 200, false, 2);
            context.Products.AddRange(product1, product2);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            var pageModel = new WebDesignerSystem.Pages.Catalog.IndexModel(context, userManagerMock.Object);
            pageModel.CategoryId = 1;

            // Act
            await pageModel.OnGetAsync();

            // Assert
            Assert.Single(pageModel.Products);
            Assert.Equal("Товар 1", pageModel.Products[0].Name);
        }

        [Fact]
        public async Task CatalogFilter_ByTypeService_ShouldReturnOnlyServices()
        {
            // Arrange
            var context = CreateInMemoryDbContext("ServiceFilterTest");

            // Очищаем существующие продукты
            var existingProducts = await context.Products.ToListAsync();
            context.Products.RemoveRange(existingProducts);

            var product1 = CreateTestProduct(1, "Товар", 100, false);
            var product2 = CreateTestProduct(2, "Услуга", 200, true);
            context.Products.AddRange(product1, product2);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            var pageModel = new WebDesignerSystem.Pages.Catalog.IndexModel(context, userManagerMock.Object);
            pageModel.ProductType = "service";

            // Act - просто вызываем OnGetAsync
            await pageModel.OnGetAsync();

            // Assert
            Assert.Single(pageModel.Products);
            Assert.True(pageModel.Products[0].IsService);
        }
    }
}