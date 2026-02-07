using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class LoginTests : IClassFixture<TestBase>
    {
        private readonly TestBase _factory;

        public LoginTests(TestBase factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Login_WrongPassword_ShouldFail()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<User>>();

            var user = new User
            {
                UserName = "wrongpass@test.com",
                Email = "wrongpass@test.com",
                FullName = "Wrong Pass User",
                EmailConfirmed = true,
                RoleId = 1 // Добавляем RoleId
            };

            await userManager.CreateAsync(user, "CorrectPassword123!");

            // Act - пробуем войти с неправильным паролем
            var result = await signInManager.PasswordSignInAsync(
                user.UserName,
                "WrongPassword!",
                isPersistent: false,
                lockoutOnFailure: false);

            // Assert
            Assert.False(result.Succeeded);
            Assert.True(result.IsNotAllowed || !result.Succeeded);
            Assert.Equal(1, user.RoleId); // Проверяем кастомное поле
        }

        [Fact]
        public async Task Login_NonExistentUser_ShouldFail()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<User>>();

            // Act - пробуем войти с несуществующим пользователем
            var result = await signInManager.PasswordSignInAsync(
                "nonexistent@test.com",
                "SomePassword123!",
                isPersistent: false,
                lockoutOnFailure: false);

            // Assert
            Assert.False(result.Succeeded);
        }
    }
}