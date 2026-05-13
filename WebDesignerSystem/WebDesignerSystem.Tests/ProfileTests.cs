using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using WebDesignerSystem.Models.Entities;
using WebDesignerSystem.Pages.Admin.Profile;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class ProfileTests : TestBase
    {
        [Fact]
        public async Task DesignerProfile_Update_ShouldSaveChanges()
        {
            // Arrange
            var context = CreateInMemoryDbContext("DesignerProfileUpdateTest");

            // Создаем существующий профиль
            var existingProfile = new DesignerProfile
            {
                Id = 1,
                Name = "Старое имя",
                Position = "Дизайнер",
                Bio = "Старое описание",
                Email = "old@test.com",
                Phone = "+79991234567",
                WorkingHours = "9-18",
                UpdatedAt = DateTime.UtcNow
            };
            context.DesignerProfiles.Add(existingProfile);
            await context.SaveChangesAsync();

            var webHostEnvMock = new Mock<IWebHostEnvironment>();
            webHostEnvMock.Setup(x => x.WebRootPath).Returns(Path.GetTempPath());

            var pageModel = new IndexModel(context, webHostEnvMock.Object);
            pageModel.TempData = CreateTempData();

            await pageModel.OnGetAsync();

            // Обновляем данные
            pageModel.Profile.Name = "Новое имя";
            pageModel.Profile.Position = "Старший дизайнер";
            pageModel.Profile.Bio = "Новое описание";
            pageModel.Profile.Email = "new@test.com";
            pageModel.Profile.Phone = "+79876543210";
            pageModel.Profile.WorkingHours = "10-19";
            pageModel.PhotoFile = null; // Не загружаем новое фото

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var updatedProfile = await context.DesignerProfiles.FirstOrDefaultAsync();
            Assert.NotNull(updatedProfile);
            Assert.Equal("Новое имя", updatedProfile.Name);
            Assert.Equal("Старший дизайнер", updatedProfile.Position);
            Assert.Equal("Новое описание", updatedProfile.Bio);
            Assert.Equal("new@test.com", updatedProfile.Email);
            Assert.Equal("+79876543210", updatedProfile.Phone);
            Assert.Equal("10-19", updatedProfile.WorkingHours);
        }

        [Fact]
        public async Task DesignerProfile_CreateNew_WhenNotExists()
        {
            // Arrange
            var context = CreateInMemoryDbContext("DesignerProfileCreateTest");

            // Удаляем все существующие профили
            var allProfiles = await context.DesignerProfiles.ToListAsync();
            context.DesignerProfiles.RemoveRange(allProfiles);
            await context.SaveChangesAsync();

            var webHostEnvMock = new Mock<IWebHostEnvironment>();
            webHostEnvMock.Setup(x => x.WebRootPath).Returns(Path.GetTempPath());

            var pageModel = new IndexModel(context, webHostEnvMock.Object);
            pageModel.TempData = CreateTempData();

            await pageModel.OnGetAsync();

            // Устанавливаем данные нового профиля
            pageModel.Profile = new DesignerProfile
            {
                Name = "Новый дизайнер",
                Position = "Дизайнер",
                Bio = "Описание",
                Email = "new@test.com",
                Phone = "+79991234567",
                WorkingHours = "10-19",
                UpdatedAt = DateTime.UtcNow
            };
            pageModel.PhotoFile = null;

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var profile = await context.DesignerProfiles.FirstOrDefaultAsync();
            Assert.NotNull(profile);
            Assert.Equal("Новый дизайнер", profile.Name);
            Assert.Equal("Дизайнер", profile.Position);
            Assert.Equal("Описание", profile.Bio);
            Assert.Equal("new@test.com", profile.Email);
            Assert.Equal("+79991234567", profile.Phone);
            Assert.Equal("10-19", profile.WorkingHours);
        }

        [Fact]
        public async Task ClientProfile_Update_ShouldSaveChanges()
        {
            // Arrange
            var context = CreateInMemoryDbContext("ClientProfileUpdateTest");
            var userId = "test-client-id";
            var user = CreateTestUserEntity(userId);
            context.Users.Add(user);

            var existingProfile = new ClientProfile
            {
                Id = 1,
                UserId = userId,
                FullName = "Старое имя",
                Phone = "+79991234567",
                DeliveryAddress = "Старый адрес",
                UpdatedAt = DateTime.UtcNow
            };
            context.ClientProfiles.Add(existingProfile);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            var pageModel = new WebDesignerSystem.Pages.Client.Profile.IndexModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);
            await pageModel.OnGetAsync();

            pageModel.Profile.FullName = "Новое имя";
            pageModel.Profile.Phone = "+79876543210";
            pageModel.Profile.DeliveryAddress = "Новый адрес";

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var updatedProfile = await context.ClientProfiles.FirstOrDefaultAsync();
            Assert.NotNull(updatedProfile);
            Assert.Equal("Новое имя", updatedProfile.FullName);
            Assert.Equal("+79876543210", updatedProfile.Phone);
            Assert.Equal("Новый адрес", updatedProfile.DeliveryAddress);
        }

        [Fact]
        public async Task ClientProfile_CreateNew_WhenNotExists()
        {
            // Arrange
            var context = CreateInMemoryDbContext("ClientProfileCreateTest");
            var userId = "test-client-id";
            var user = CreateTestUserEntity(userId);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var userManagerMock = CreateMockUserManager();
            userManagerMock.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);
            userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .ReturnsAsync(IdentityResult.Success);

            var pageModel = new WebDesignerSystem.Pages.Client.Profile.IndexModel(context, userManagerMock.Object);
            var tempData = CreateTempData();
            var principal = CreateTestUser(userId);

            SetupPageModelContext(pageModel, principal, tempData);
            await pageModel.OnGetAsync();

            pageModel.Profile = new ClientProfile
            {
                UserId = userId,
                FullName = "Новый клиент",
                Phone = "+79991234567",
                DeliveryAddress = "Новый адрес",
                UpdatedAt = DateTime.UtcNow
            };

            // Act
            var result = await pageModel.OnPostAsync();

            // Assert
            var profile = await context.ClientProfiles.FirstOrDefaultAsync();
            Assert.NotNull(profile);
            Assert.Equal("Новый клиент", profile.FullName);
            Assert.Equal("+79991234567", profile.Phone);
            Assert.Equal("Новый адрес", profile.DeliveryAddress);
        }
    }
}