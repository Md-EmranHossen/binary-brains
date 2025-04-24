using AmarTech.Domain.Entities;
using AmarTech.Infrastructure;
using AmarTech.Infrastructure.DbInitializer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AmarTech.Test
{
    public class DbInitializerTest
    {
        // Helper method to create a new in-memory database for each test
        private static ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB per test
                .Options;
            return new ApplicationDbContext(options);
        }

        // Helper method to create a mock UserManager
        private static Mock<UserManager<IdentityUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<IdentityUser>>();
            return new Mock<UserManager<IdentityUser>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        }

        // Helper method to create a mock RoleManager
        private static Mock<RoleManager<IdentityRole>> GetMockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(store.Object, null!, null!, null!, null!);
        }

        [Fact]
        public async Task Initialize_CreatesRoles_WhenRolesDoNotExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var userManager = GetMockUserManager();
            var roleManager = GetMockRoleManager();

            roleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            roleManager.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<IdentityRole>(role =>
                {
                    context.Roles.Add(role);
                    context.SaveChanges();
                });

            var initializer = new DbInitializer(userManager.Object, roleManager.Object, context);

            // Act
            initializer.Initialize();

            // Assert
            var roles = context.Roles.ToList();
            Assert.Contains(roles, r => r.Name == SD.Role_Customer);
            Assert.Contains(roles, r => r.Name == SD.Role_Employee);
            Assert.Contains(roles, r => r.Name == SD.Role_Admin);
            Assert.Contains(roles, r => r.Name == SD.Role_Company);
        }

        [Fact]
        public async Task Initialize_DoesNotCreateRoles_WhenRolesExist()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var userManager = GetMockUserManager();
            var roleManager = GetMockRoleManager();

            roleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(true);
            roleManager.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).Throws(new InvalidOperationException("Should not be called"));

            var initializer = new DbInitializer(userManager.Object, roleManager.Object, context);

            // Act
            initializer.Initialize();

            // Assert
            roleManager.Verify(r => r.CreateAsync(It.IsAny<IdentityRole>()), Times.Never());
        }

        [Fact]
        public async Task Initialize_CreatesAdminUser_WithCorrectPropertiesAndRole()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var userManager = GetMockUserManager();
            var roleManager = GetMockRoleManager();

            roleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);
            roleManager.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<IdentityRole>(role =>
                {
                    context.Roles.Add(role);
                    context.SaveChanges();
                });

            userManager.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success)
                .Callback<IdentityUser, string>((user, _) =>
                {
                    context.ApplicationUsers.Add((ApplicationUser)user);
                    context.SaveChanges();
                });

            userManager.Setup(u => u.AddToRoleAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var initializer = new DbInitializer(userManager.Object, roleManager.Object, context);

            // Act
            initializer.Initialize();

            // Assert
            var adminUser = context.ApplicationUsers.FirstOrDefault(u => u.Email == "admin@binarybrains.com");
            Assert.NotNull(adminUser);
            Assert.Equal("admin@binarybrains.com", adminUser.UserName);
            Assert.Equal("Emran Hossen", adminUser.Name);
            Assert.Equal("01794307576", adminUser.PhoneNumber);
            Assert.Equal("Murpur 2", adminUser.StreetAddress);
            Assert.Equal("Dhaka", adminUser.State);
            Assert.Equal("23422", adminUser.PostalCode);
            Assert.Equal("Dhaka", adminUser.City);

            userManager.Verify(u => u.AddToRoleAsync(It.Is<IdentityUser>(user => user.Email == "admin@binarybrains.com"), SD.Role_Admin), Times.Once());
        }

        [Fact]
        public void Initialize_DoesNotThrow_WhenMigrationLogicIsExecuted()
        {
            // Arrange
            var context = GetInMemoryDbContext();
            var userManager = GetMockUserManager();
            var roleManager = GetMockRoleManager();

            // No specific setup for migrations since in-memory DB doesn't support them
            var initializer = new DbInitializer(userManager.Object, roleManager.Object, context);

            // Act & Assert
            var exception = Record.Exception(() => initializer.Initialize());
            Assert.Null(exception); // Ensure no exception is thrown
        }
    }
}