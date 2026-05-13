using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using WebDesignerSystem.Models.Entities;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class CartTests : TestBase
    {
        [Fact]
        public async Task AddToCart_WhenProductExists_ShouldAddItemToCart()
        {
            // Arrange
            var context = CreateInMemoryDbContext("AddToCartTest");
            var userId = "test-user-1";
            var user = CreateTestUserEntity(userId);
            context.Users.Add(user);

            var product = CreateTestProduct(1, "Тестовый товар", 500, false);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new WebDesignerSystem.Pages.Catalog.IndexModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);

            // Act
            var result = await pageModel.OnPostAddToCartAsync(1);

            // Assert
            var cartItem = await context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == 1);
            Assert.NotNull(cartItem);
            Assert.Equal(1, cartItem.Quantity);
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task AddToCart_WhenProductAlreadyInCart_ShouldIncreaseQuantity()
        {
            // Arrange
            var context = CreateInMemoryDbContext("IncreaseQuantityTest");
            var userId = "test-user-1";
            var user = CreateTestUserEntity(userId);
            context.Users.Add(user);

            var product = CreateTestProduct(1, "Тестовый товар", 500, false);
            context.Products.Add(product);

            var existingCartItem = new CartItem
            {
                UserId = userId,
                ProductId = 1,
                Quantity = 2,
                AddedAt = DateTime.UtcNow
            };
            context.CartItems.Add(existingCartItem);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new WebDesignerSystem.Pages.Catalog.IndexModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);

            // Act
            var result = await pageModel.OnPostAddToCartAsync(1);

            // Assert
            var cartItem = await context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == 1);
            Assert.NotNull(cartItem);
            Assert.Equal(3, cartItem.Quantity);
        }

        [Fact]
        public async Task AddToCart_WhenProductIsService_ShouldNotAddToCart()
        {
            // Arrange
            var context = CreateInMemoryDbContext("ServiceNotAddTest");
            var userId = "test-user-1";
            var user = CreateTestUserEntity(userId);
            context.Users.Add(user);

            var service = CreateTestProduct(1, "Тестовая услуга", 1000, true);
            context.Products.Add(service);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new WebDesignerSystem.Pages.Catalog.IndexModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);

            // Act
            var result = await pageModel.OnPostAddToCartAsync(1);

            // Assert
            var cartItem = await context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == 1);
            Assert.Null(cartItem);
            Assert.IsType<RedirectToPageResult>(result);
        }

        [Fact]
        public async Task RemoveFromCart_ShouldDeleteItem()
        {
            // Arrange
            var context = CreateInMemoryDbContext("RemoveFromCartTest");
            var userId = "test-user-1";
            var user = CreateTestUserEntity(userId);
            context.Users.Add(user);

            var cartItem = new CartItem
            {
                Id = 1,
                UserId = userId,
                ProductId = 1,
                Quantity = 1,
                AddedAt = DateTime.UtcNow
            };
            context.CartItems.Add(cartItem);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new WebDesignerSystem.Pages.Client.Cart.IndexModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);

            // Act
            var result = await pageModel.OnPostRemoveAsync(1);

            // Assert
            var cartItemAfter = await context.CartItems.FirstOrDefaultAsync(c => c.Id == 1);
            Assert.Null(cartItemAfter);
        }

        [Fact]
        public async Task UpdateQuantity_ShouldChangeItemQuantity()
        {
            // Arrange
            var context = CreateInMemoryDbContext("UpdateQuantityTest");
            var userId = "test-user-1";
            var user = CreateTestUserEntity(userId);
            context.Users.Add(user);

            var cartItem = new CartItem
            {
                Id = 1,
                UserId = userId,
                ProductId = 1,
                Quantity = 1,
                AddedAt = DateTime.UtcNow
            };
            context.CartItems.Add(cartItem);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new WebDesignerSystem.Pages.Client.Cart.IndexModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);

            // Act
            var result = await pageModel.OnPostUpdateQuantityAsync(1, 5);

            // Assert
            var updatedItem = await context.CartItems.FindAsync(1);
            Assert.Equal(5, updatedItem.Quantity);
        }

        [Fact]
        public async Task UpdateQuantity_WhenQuantityLessThan1_ShouldSetTo1()
        {
            // Arrange
            var context = CreateInMemoryDbContext("UpdateQuantityMinTest");
            var userId = "test-user-1";
            var user = CreateTestUserEntity(userId);
            context.Users.Add(user);

            var cartItem = new CartItem
            {
                Id = 1,
                UserId = userId,
                ProductId = 1,
                Quantity = 5,
                AddedAt = DateTime.UtcNow
            };
            context.CartItems.Add(cartItem);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new WebDesignerSystem.Pages.Client.Cart.IndexModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);

            // Act
            var result = await pageModel.OnPostUpdateQuantityAsync(1, 0);

            // Assert
            var updatedItem = await context.CartItems.FindAsync(1);
            Assert.Equal(1, updatedItem.Quantity);
        }
    }
}