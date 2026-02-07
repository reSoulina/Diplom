using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class AuthTests : IClassFixture<TestBase>
    {
        private readonly TestBase _factory;

        public AuthTests(TestBase factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Register_ValidClient_ShouldCreateUser()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Act - эмулируем регистрацию через UserManager
            var newUser = new User
            {
                UserName = "newclient@test.com",
                Email = "newclient@test.com",
                FullName = "Новый Клиент",
                EmailConfirmed = true,
                RoleId = 1 // Client (должна существовать в таблице Roles)
            };

            var result = await userManager.CreateAsync(newUser, "Test123!");

            // Assert
            Assert.True(result.Succeeded);

            var createdUser = await userManager.FindByEmailAsync("newclient@test.com");
            Assert.NotNull(createdUser);
            Assert.Equal("Новый Клиент", createdUser.FullName);
            Assert.Equal(1, createdUser.RoleId);
        }

        [Fact]
        public async Task Register_WeakPassword_ShouldFail()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            var testUser = new User
            {
                UserName = "weakpass@test.com",
                Email = "weakpass@test.com",
                FullName = "Weak Password User",
                EmailConfirmed = true,
                RoleId = 1
            };

            // Act - пробуем слишком простой пароль
            var result = await userManager.CreateAsync(testUser, "1"); // слишком короткий

            // Assert
            Assert.False(result.Succeeded);
            Assert.Contains(result.Errors, e => e.Description.Contains("Password"));
        }

        [Fact]
        public async Task Register_Client_ShouldHaveClientRole()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Создаем роль Client если ее нет в Identity
            if (!await roleManager.RoleExistsAsync("Client"))
            {
                await roleManager.CreateAsync(new IdentityRole("Client"));
            }

            var clientUser = new User
            {
                UserName = "clientrole@test.com",
                Email = "clientrole@test.com",
                FullName = "Client Role Test",
                EmailConfirmed = true,
                RoleId = 1 // Client из кастомной таблицы Roles
            };

            // Act
            var createResult = await userManager.CreateAsync(clientUser, "Password123!");

            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(clientUser, "Client");
            }

            // Assert
            Assert.True(createResult.Succeeded);

            var roles = await userManager.GetRolesAsync(clientUser);
            Assert.Contains("Client", roles);
            Assert.Equal(1, clientUser.RoleId); // Проверяем кастомное поле RoleId
        }
    }
}