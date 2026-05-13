using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Reflection;
using System.Security.Claims;
using WebDesignerSystem.Models.Entities;
using WebDesignerSystem.Pages.Admin.Appointments;
using WebDesignerSystem.Pages.Client;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class AppointmentTests : TestBase
    {
        [Fact]
        public async Task BookService_ShouldCreateAppointment()
        {
            // Arrange
            var context = CreateInMemoryDbContext("BookServiceTest");
            var userId = "test-client-id";
            var user = CreateTestUserEntity(userId, "client@test.com", "Тестовый Клиент", 1);
            context.Users.Add(user);

            var service = CreateTestProduct(1, "Тестовая услуга", 1500, true);
            context.Products.Add(service);

            var clientProfile = new ClientProfile
            {
                UserId = userId,
                FullName = "Тестовый Клиент",
                Phone = "+79991234567"
            };
            context.ClientProfiles.Add(clientProfile);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new BookServiceModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId, "client@test.com", "Client");

            SetupPageModelContext(pageModel, principal, tempData);

            await pageModel.OnGetAsync(1);

            pageModel.Input = new BookServiceModel.InputModel
            {
                AppointmentDate = DateTime.UtcNow.AddDays(2).Date,
                AppointmentTime = "10:00",
                Format = "online",
                ContactInfo = "+79991234567",
                Notes = "Тестовая запись"
            };

            // Act
            var result = await pageModel.OnPostAsync(1);

            // Assert
            var appointment = await context.ServiceAppointments.FirstOrDefaultAsync();
            Assert.NotNull(appointment);
            Assert.Equal(userId, appointment.ClientId);
            Assert.Equal(1, appointment.ServiceId);
            Assert.Equal("pending", appointment.Status);
            Assert.Equal("online", appointment.Format);
        }

        [Fact]
        public async Task BookService_WhenTimeSlotIsTaken_ShouldReturnError()
        {
            // Arrange
            var context = CreateInMemoryDbContext("TimeSlotTakenTest");
            var userId = "test-client-id";
            var user = CreateTestUserEntity(userId);
            context.Users.Add(user);

            var service = CreateTestProduct(1, "Тестовая услуга", 1500, true);
            context.Products.Add(service);

            var appointmentDateTime = DateTime.UtcNow.AddDays(2).Date.AddHours(10);
            var existingAppointment = new ServiceAppointment
            {
                ClientId = "other-user",
                ServiceId = 1,
                AppointmentDateTime = appointmentDateTime,
                Status = "pending",
                Format = "online",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ServiceAppointments.Add(existingAppointment);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new BookServiceModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);

            await pageModel.OnGetAsync(1);

            pageModel.Input = new BookServiceModel.InputModel
            {
                AppointmentDate = appointmentDateTime.Date,
                AppointmentTime = "10:00",
                Format = "online",
                ContactInfo = "+79991234567"
            };

            // Act
            var result = await pageModel.OnPostAsync(1);

            // Assert
            Assert.True(pageModel.ModelState.ContainsKey("Input.AppointmentTime"));
        }

        [Fact]
        public async Task CancelAppointment_ShouldSetStatusToCancelled()
        {
            // Arrange
            var context = CreateInMemoryDbContext("CancelAppointmentTest");
            var userId = "test-client-id";
            var user = CreateTestUserEntity(userId);
            context.Users.Add(user);

            var appointment = new ServiceAppointment
            {
                Id = 1,
                ClientId = userId,
                ServiceId = 1,
                AppointmentDateTime = DateTime.UtcNow.AddDays(2),
                Status = "pending",
                Format = "online",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ServiceAppointments.Add(appointment);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new MyAppointmentsModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);

            // Act
            var result = await pageModel.OnPostCancelAsync(1);

            // Assert
            var updatedAppointment = await context.ServiceAppointments.FindAsync(1);
            Assert.Equal("cancelled", updatedAppointment.Status);
        }

        [Fact]
        public async Task CancelAppointment_WhenStatusNotPending_ShouldNotCancel()
        {
            // Arrange
            var context = CreateInMemoryDbContext("CancelConfirmedTest");
            var userId = "test-client-id";
            var user = CreateTestUserEntity(userId);
            context.Users.Add(user);

            var appointment = new ServiceAppointment
            {
                Id = 1,
                ClientId = userId,
                ServiceId = 1,
                AppointmentDateTime = DateTime.UtcNow.AddDays(2),
                Status = "confirmed",
                Format = "online",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ServiceAppointments.Add(appointment);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var pageModel = new MyAppointmentsModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);

            // Act
            var result = await pageModel.OnPostCancelAsync(1);

            // Assert
            var updatedAppointment = await context.ServiceAppointments.FindAsync(1);
            Assert.Equal("confirmed", updatedAppointment.Status);
        }

        [Fact]
        public async Task GetAvailableTimeSlots_ShouldReturnOnlyFreeSlots()
        {
            // Arrange
            var context = CreateInMemoryDbContext("TimeSlotsTest");
            var testDate = DateTime.UtcNow.AddDays(1).Date;

            var bookingTime = testDate.AddHours(10);
            var existingAppointment = new ServiceAppointment
            {
                ClientId = "user1",
                ServiceId = 1,
                AppointmentDateTime = bookingTime,
                Status = "pending",
                Format = "online",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            context.ServiceAppointments.Add(existingAppointment);
            await context.SaveChangesAsync();

            // Используем реальный UserManager через Mock
            var userManagerMock = CreateMockUserManager();
            var pageModel = new BookServiceModel(context, userManagerMock.Object);

            var method = typeof(BookServiceModel).GetMethod("GetAvailableTimeSlots",
                BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            var slots = await (Task<List<BookServiceModel.TimeSlot>>)method.Invoke(pageModel, new object[] { testDate });

            // Assert
            Assert.NotNull(slots);
            Assert.Contains(slots, s => s.Time == "09:00");
            Assert.DoesNotContain(slots, s => s.Time == "10:00");
        }
    }
}