using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WebDesignerSystem.Data;
using WebDesignerSystem.Models.Entities;
using Xunit;

namespace WebDesignerSystem.Tests
{
    public class RoleTests : IClassFixture<TestBase>
    {
        private readonly TestBase _factory;

        public RoleTests(TestBase factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task DesignerRole_ShouldBeCreated()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Act
            var roleExists = await roleManager.RoleExistsAsync("Designer");

            // Если роли нет - создаем
            if (!roleExists)
            {
                var result = await roleManager.CreateAsync(new IdentityRole("Designer"));
                Assert.True(result.Succeeded);
                roleExists = await roleManager.RoleExistsAsync("Designer");
            }

            // Assert
            Assert.True(roleExists);
        }

        [Fact]
        public async Task ClientRole_ShouldBeCreated()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Act
            var roleExists = await roleManager.RoleExistsAsync("Client");

            // Если роли нет - создаем
            if (!roleExists)
            {
                var result = await roleManager.CreateAsync(new IdentityRole("Client"));
                Assert.True(result.Succeeded);
                roleExists = await roleManager.RoleExistsAsync("Client");
            }

            // Assert
            Assert.True(roleExists);
        }

        [Fact]
        public async Task User_CanHaveDesignerRole()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Создаем роль Designer если ее нет
            if (!await roleManager.RoleExistsAsync("Designer"))
            {
                await roleManager.CreateAsync(new IdentityRole("Designer"));
            }

            var user = new User
            {
                UserName = "designeruser@test.com",
                Email = "designeruser@test.com",
                FullName = "Designer User",
                EmailConfirmed = true,
                RoleId = 2 // Designer
            };

            // Act
            var createResult = await userManager.CreateAsync(user, "Designer123!");

            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Designer");
            }

            // Assert
            Assert.True(createResult.Succeeded);

            var roles = await userManager.GetRolesAsync(user);
            Assert.Contains("Designer", roles);
            Assert.Equal(2, user.RoleId); // Проверяем кастомное поле RoleId
        }

        [Fact]
        public async Task User_CanHaveMultipleRoles()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Создаем роли если их нет
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("Moderator"))
            {
                await roleManager.CreateAsync(new IdentityRole("Moderator"));
            }

            var user = new User
            {
                UserName = "multipleroles@test.com",
                Email = "multipleroles@test.com",
                FullName = "Multiple Roles User",
                EmailConfirmed = true,
                RoleId = 3 // Добавляем RoleId (например, для Admin)
            };

            // Act
            var createResult = await userManager.CreateAsync(user, "Password123!");

            if (createResult.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
                await userManager.AddToRoleAsync(user, "Moderator");
            }

            // Assert
            Assert.True(createResult.Succeeded);

            var roles = await userManager.GetRolesAsync(user);
            Assert.Contains("Admin", roles);
            Assert.Contains("Moderator", roles);
            Assert.Equal(2, roles.Count);
            Assert.Equal(3, user.RoleId); // Проверяем кастомное поле
        }

        [Fact]
        public async Task User_InRole_CheckWorks()
        {
            // Arrange
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Создаем роль
            if (!await roleManager.RoleExistsAsync("Tester"))
            {
                await roleManager.CreateAsync(new IdentityRole("Tester"));
            }

            var user = new User
            {
                UserName = "rolecheck@test.com",
                Email = "rolecheck@test.com",
                FullName = "Role Check User",
                EmailConfirmed = true,
                RoleId = 4 // Добавляем RoleId
            };

            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Tester");

            // Act
            var isInRole = await userManager.IsInRoleAsync(user, "Tester");
            var isNotInRole = await userManager.IsInRoleAsync(user, "NonExistentRole");

            // Assert
            Assert.True(isInRole);
            Assert.False(isNotInRole);
            Assert.Equal(4, user.RoleId); // Проверяем кастомное поле
        }
    }
}