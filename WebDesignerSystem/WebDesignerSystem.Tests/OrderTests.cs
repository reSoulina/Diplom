using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using WebDesignerSystem.Models.Entities;
using WebDesignerSystem.Pages.Admin.Orders;
using WebDesignerSystem.Pages.Client;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class OrderTests : TestBase
    {
        [Fact]
        public async Task Checkout_CreateOrder_ShouldCreateOrderAndClearCart()
        {
            // Arrange
            var context = CreateInMemoryDbContext("CheckoutTest");
            var userId = "test-client-id";
            var user = CreateTestUserEntity(userId, "client@test.com", "Тестовый Клиент", 1);
            context.Users.Add(user);

            var product = CreateTestProduct(1, "Товар для заказа", 1000, false);
            context.Products.Add(product);

            var cartItems = new List<CartItem>
            {
                new CartItem { UserId = userId, ProductId = 1, Quantity = 2, AddedAt = DateTime.UtcNow }
            };
            context.CartItems.AddRange(cartItems);

            var clientProfile = new ClientProfile
            {
                UserId = userId,
                FullName = "Тестовый Клиент",
                Phone = "+79991234567",
                DeliveryAddress = "г. Тест, ул. Тестовая, 1"
            };
            context.ClientProfiles.Add(clientProfile);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new CheckoutModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);

            await pageModel.OnGetAsync();
            pageModel.Input = new CheckoutModel.OrderInput
            {
                FullName = "Тестовый Клиент",
                Phone = "+79991234567",
                Address = "г. Тест, ул. Тестовая, 1",
                Notes = "Тестовый заказ"
            };

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var order = await context.Orders.FirstOrDefaultAsync();
            Assert.NotNull(order);
            Assert.Equal(userId, order.ClientId);
            Assert.Equal(2000, order.TotalAmount);
            Assert.Equal(1, order.CurrentStatusId);

            var orderItems = await context.OrderItems.Where(oi => oi.OrderId == order.Id).ToListAsync();
            Assert.Single(orderItems);
            Assert.Equal(2, orderItems[0].Quantity);

            var cartAfter = await context.CartItems.Where(c => c.UserId == userId).ToListAsync();
            Assert.Empty(cartAfter);

            var statusHistory = await context.OrderStatusHistories.FirstOrDefaultAsync();
            Assert.NotNull(statusHistory);
            Assert.Equal("Заказ создан", statusHistory.Comment);
        }

        [Fact]
        public async Task Checkout_WhenCartEmpty_ShouldNotCreateOrder()
        {
            // Arrange
            var context = CreateInMemoryDbContext("EmptyCartTest");
            var userId = "test-client-id";
            var user = CreateTestUserEntity(userId);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new CheckoutModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);

            // Act
            var result = await pageModel.OnGetAsync();

            // Assert
            Assert.IsType<RedirectToPageResult>(result);
            var redirectResult = result as RedirectToPageResult;
            Assert.Equal("/Client/Cart/Index", redirectResult?.PageName);
        }

        [Fact]
        public async Task OrderStatusChange_ShouldAddHistoryRecord()
        {
            // Arrange
            var context = CreateInMemoryDbContext("StatusChangeTest");
            var userId = "test-designer-id";
            var clientId = "test-client-id";
            var user = CreateTestUserEntity(userId, "designer@test.com", "Тестовый Дизайнер", 2);
            context.Users.Add(user);

            var order = new Order
            {
                Id = 1,
                ClientId = clientId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 1000,
                CurrentStatusId = 1,
                UpdatedAt = DateTime.UtcNow
            };
            context.Orders.Add(order);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new DetailsModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId, "designer@test.com", "Designer");

            SetupPageModelContext(pageModel, principal, tempData);

            // Act
            var result = await pageModel.OnPostChangeStatusAsync(1, 2, "Начат процесс выполнения");

            // Assert
            var updatedOrder = await context.Orders.FindAsync(1);
            Assert.Equal(2, updatedOrder.CurrentStatusId);

            var history = await context.OrderStatusHistories.FirstOrDefaultAsync();
            Assert.NotNull(history);
            Assert.Equal(1, history.OrderId);
            Assert.Equal(2, history.StatusId);
            Assert.Equal(userId, history.ChangedBy);
            Assert.Equal("Начат процесс выполнения", history.Comment);
        }
    }
}