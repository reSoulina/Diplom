using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebDesignerSystem.Models.Entities;
using WebDesignerSystem.Pages.Admin.Categories;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class CategoryTests : TestBase
    {
        [Fact]
        public async Task CreateCategory_ShouldAddToDatabase()
        {
            // Arrange
            var context = CreateInMemoryDbContext("CreateCategoryTest");

            var pageModel = new CreateModel(context);

            // Добавляем TempData для страницы
            pageModel.TempData = CreateTempData();

            pageModel.Category = new Category
            {
                Name = "Новая уникальная категория",
                Description = "Описание новой категории"
            };

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var category = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Новая уникальная категория");
            Assert.NotNull(category);
            Assert.Equal("Описание новой категории", category.Description);
        }

        [Fact]
        public async Task EditCategory_ShouldUpdateExistingCategory()
        {
            // Arrange
            var context = CreateInMemoryDbContext("EditCategoryTest");

            // Создаем категорию с уникальным Id
            var existingCategory = new Category
            {
                Id = 100,
                Name = "Старая категория",
                Description = "Старое описание",
                CreatedAt = DateTime.UtcNow
            };
            context.Categories.Add(existingCategory);
            await context.SaveChangesAsync();

            var pageModel = new EditModel(context);

            // Добавляем TempData для страницы
            pageModel.TempData = CreateTempData();

            await pageModel.OnGetAsync(100);

            pageModel.Category.Name = "Обновленная категория";
            pageModel.Category.Description = "Новое описание";

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var updatedCategory = await context.Categories.FindAsync(100);
            Assert.NotNull(updatedCategory);
            Assert.Equal("Обновленная категория", updatedCategory.Name);
            Assert.Equal("Новое описание", updatedCategory.Description);
        }

        [Fact]
        public async Task DeleteCategory_WhenNoProducts_ShouldRemove()
        {
            // Arrange
            var context = CreateInMemoryDbContext("DeleteCategoryTest");

            // Создаем категорию без товаров
            var category = new Category
            {
                Id = 200,
                Name = "Удаляемая категория",
                Description = "Описание",
                CreatedAt = DateTime.UtcNow
            };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var pageModel = new DeleteModel(context);

            // Добавляем TempData для страницы
            pageModel.TempData = CreateTempData();

            // Act
            var result = await pageModel.OnPostAsync(200);

            // Assert
            var deletedCategory = await context.Categories.FindAsync(200);
            Assert.Null(deletedCategory);
        }

        [Fact]
        public async Task DeleteCategory_WhenHasProducts_ShouldNotRemove()
        {
            // Arrange
            var context = CreateInMemoryDbContext("DeleteCategoryWithProductsTest");

            // Создаем категорию с товарами
            var category = new Category
            {
                Id = 300,
                Name = "Категория с товарами",
                Description = "Описание",
                CreatedAt = DateTime.UtcNow
            };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            var product = new Product
            {
                Id = 300,
                Name = "Товар в категории",
                Description = "Описание товара",
                Price = 100,
                CategoryId = 300,
                IsService = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var pageModel = new DeleteModel(context);

            // Добавляем TempData для страницы
            pageModel.TempData = CreateTempData();

            await pageModel.OnGetAsync(300);

            // Act
            var result = await pageModel.OnPostAsync(300);

            // Assert
            var categoryStillExists = await context.Categories.FindAsync(300);
            Assert.NotNull(categoryStillExists);
        }

        [Fact]
        public async Task DeleteCategory_Get_ShouldShowProductCount()
        {
            // Arrange
            var context = CreateInMemoryDbContext("DeleteCategoryProductCountTest");

            // Создаем категорию
            var category = new Category
            {
                Id = 400,
                Name = "Категория для подсчета",
                Description = "Описание",
                CreatedAt = DateTime.UtcNow
            };
            context.Categories.Add(category);
            await context.SaveChangesAsync();

            // Добавляем товары в эту категорию
            var product1 = new Product
            {
                Id = 401,
                Name = "Товар 1",
                Description = "Описание 1",
                Price = 100,
                CategoryId = 400,
                IsService = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var product2 = new Product
            {
                Id = 402,
                Name = "Товар 2",
                Description = "Описание 2",
                Price = 200,
                CategoryId = 400,
                IsService = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            context.Products.AddRange(product1, product2);
            await context.SaveChangesAsync();

            var pageModel = new DeleteModel(context);

            // Добавляем TempData для страницы
            pageModel.TempData = CreateTempData();

            // Act
            await pageModel.OnGetAsync(400);

            // Assert
            Assert.Equal(2, pageModel.ProductCount);
            Assert.NotNull(pageModel.Category);
            Assert.Equal("Категория для подсчета", pageModel.Category.Name);
        }
    }
}