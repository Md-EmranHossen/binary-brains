using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using ECommerceWebApp.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ECommerceSystem.Test.ControllerTests
{
    public class UserControllerTests
    {
        private readonly Mock<IApplicationUserService> _mockApplicationUserService;
        private readonly Mock<ICompanyService> _mockCompanyService;
        private readonly Mock<UserManager<IdentityUser>>? _mockUserManager;
        private readonly UserController _controller;

        public UserControllerTests()
        {
            _mockApplicationUserService = new Mock<IApplicationUserService>();
            _mockCompanyService = new Mock<ICompanyService>();

            // Setup UserManager mock (requires special setup due to its abstraction)
            var store = new Mock<IUserStore<IdentityUser>>();
            _mockUserManager = new Mock<UserManager<IdentityUser>>(
    store.Object,
    new Mock<IOptions<IdentityOptions>>().Object,
    new Mock<IPasswordHasher<IdentityUser>>().Object,
    new IUserValidator<IdentityUser>[0],
    new IPasswordValidator<IdentityUser>[0],
    new Mock<ILookupNormalizer>().Object,
    new Mock<IdentityErrorDescriber>().Object,
    new Mock<IServiceProvider>().Object,
    new Mock<ILogger<UserManager<IdentityUser>>>().Object
);

            _controller = new UserController(
                _mockApplicationUserService.Object,
                _mockCompanyService.Object,
                _mockUserManager.Object
            );
        }

        [Fact]
        public void Index_ReturnsViewWithUserList()
        {
            // Arrange
            var testUsers = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", Name = "User1" },
                new ApplicationUser { Id = "2", Name = "User2" }
            };

            _mockApplicationUserService.Setup(service => service.GetAllUsers())
                .Returns(testUsers);

            _mockApplicationUserService.Setup(service => service.GetUserrole(It.IsAny<string>()))
                .Returns("Admin");

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<ApplicationUser>>(result.Model);
            Assert.Equal(2, model.Count);
            Assert.Equal("Admin", model[0].Role);
            Assert.Equal("Admin", model[1].Role);
        }

        [Fact]
        public void LockUnlock_WithNullUserId_ReturnsNotFound()
        {
            // Arrange
            _mockApplicationUserService.Setup(service => service.GetUserById(null))
                .Returns((ApplicationUser?)null);

            // Act
            var result = _controller.LockUnlock(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void LockUnlock_WithLockedUser_UnlocksUser()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser
            {
                Id = userId,
                Name = "User1",
                LockoutEnd = DateTime.Now.AddDays(5) // User is locked
            };

            _mockApplicationUserService.Setup(service => service.GetUserById(userId))
                .Returns(user);

            _mockApplicationUserService.Setup(service => service.UpdateUser(It.IsAny<ApplicationUser>()))
                .Verifiable();

            // Act
            var result = _controller.LockUnlock(userId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.True(user.LockoutEnd <= DateTime.Now); // Verify user was unlocked
            _mockApplicationUserService.Verify(service => service.UpdateUser(user), Times.Once);
        }

        [Fact]
        public void LockUnlock_WithUnlockedUser_LocksUser()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser
            {
                Id = userId,
                Name = "User1",
                LockoutEnd = null // User is not locked
            };

            _mockApplicationUserService.Setup(service => service.GetUserById(userId))
                .Returns(user);

            _mockApplicationUserService.Setup(service => service.UpdateUser(It.IsAny<ApplicationUser>()))
                .Verifiable();

            // Act
            var result = _controller.LockUnlock(userId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.True(user.LockoutEnd > DateTime.Now); // Verify user was locked
            _mockApplicationUserService.Verify(service => service.UpdateUser(user), Times.Once);
        }

        [Fact]
        public void RoleManagement_Get_WithNullUserId_ReturnsNotFound()
        {
            // Act
            var result = _controller.RoleManagement((string?)null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void RoleManagement_Get_WithInvalidUserId_ReturnsNotFound()
        {
            // Arrange
            string userId = "nonexistent-user";
            _mockApplicationUserService.Setup(service => service.GetUserByIdAndIncludeprop(userId, "Company"))
                .Returns((ApplicationUser?)null);

            // Act
            var result = _controller.RoleManagement(userId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void RoleManagement_Get_WithValidUserId_ReturnsViewWithRoleManagementVM()
        {
            // Arrange
            string userId = "valid-user";
            var user = new ApplicationUser { Id = userId, Name = "Test User" };
            var roleList = new List<SelectListItem>
            {
                new SelectListItem { Text = "Admin", Value = "Admin" },
                new SelectListItem { Text = "User", Value = "User" }
            };
            var companyList = new List<Company>
            {
                new Company { Id = 1, Name = "Company1" },
                new Company { Id = 2, Name = "Company2" }
            };

            _mockApplicationUserService.Setup(service => service.GetUserByIdAndIncludeprop(userId, "Company"))
                .Returns(user);
            _mockApplicationUserService.Setup(service => service.GetAllRoles())
                .Returns(roleList);
            _mockApplicationUserService.Setup(service => service.GetUserrole(userId))
                .Returns("Admin");
            _mockCompanyService.Setup(service => service.GetAllCompanies())
                .Returns(companyList);

            // Act
            var result = _controller.RoleManagement(userId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<RoleManagemantVM>(result.Model);
            Assert.Equal(userId, model.User?.Id);
            Assert.Equal("Admin", model.User?.Role);
            Assert.Equal(2, model.RoleList.Count());
            Assert.Equal(2, model.CompanyList.Count());
        }

        [Fact]
        public void RoleManagement_Post_WithInvalidModelState_ReturnsViewWithSameModel()
        {
            // Arrange
            var roleVM = new RoleManagemantVM
            {
                User = new ApplicationUser { Id = "user-123", Name = "User1", Role = "Admin" }
            };

            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = _controller.RoleManagement(roleVM) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(roleVM, result.Model);
        }

        [Fact]
        public void RoleManagement_Post_WithNonExistentUser_ReturnsNotFound()
        {
            // Arrange
            var roleVM = new RoleManagemantVM
            {
                User = new ApplicationUser { Id = "nonexistent-user", Name = "User1", Role = "Admin" }
            };

            _mockApplicationUserService.Setup(service => service.GetUserById("nonexistent-user"))
                .Returns((ApplicationUser?)null);

            // Act
            var result = _controller.RoleManagement(roleVM);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void RoleManagement_Post_WithRoleChangeToCompany_UpdatesCompanyIdAndRole()
        {
            // Arrange
            string userId = "user-123";
            var user = new ApplicationUser { Id = userId, Name = "User1" };
            var roleVM = new RoleManagemantVM
            {
                User = new ApplicationUser
                {
                    Id = userId,
                    Name = "User1",
                    Role = SD.Role_Company,
                    CompanyId = 1
                }
            };

            _mockApplicationUserService.Setup(service => service.GetUserById(userId))
                .Returns(user);
            _mockUserManager?.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });
            _mockApplicationUserService.Setup(service => service.UpdateUser(It.IsAny<ApplicationUser>()))
                .Verifiable();
            _mockUserManager?.Setup(m => m.RemoveFromRoleAsync(user, "User"))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager?.Setup(m => m.AddToRoleAsync(user, SD.Role_Company))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = _controller.RoleManagement(roleVM) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal(1, user.CompanyId);
            _mockApplicationUserService.Verify(service => service.UpdateUser(user), Times.Once);
            _mockUserManager?.Verify(m => m.RemoveFromRoleAsync(user, "User"), Times.Once);
            _mockUserManager?.Verify(m => m.AddToRoleAsync(user, SD.Role_Company), Times.Once);
        }

        [Fact]
        public void RoleManagement_Post_WithRoleChangeFromCompany_RemovesCompanyIdAndUpdatesRole()
        {
            // Arrange
            string userId = "user-123";
            var user = new ApplicationUser { Id = userId, Name = "User1", CompanyId = 1 };
            var roleVM = new RoleManagemantVM
            {
                User = new ApplicationUser
                {
                    Id = userId,
                    Name = "User1",
                    Role = "Admin"
                }
            };

            _mockApplicationUserService.Setup(service => service.GetUserById(userId))
                .Returns(user);
            _mockUserManager?.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { SD.Role_Company });
            _mockApplicationUserService.Setup(service => service.UpdateUser(It.IsAny<ApplicationUser>()))
                .Verifiable();
            _mockUserManager?.Setup(m => m.RemoveFromRoleAsync(user, SD.Role_Company))
                .ReturnsAsync(IdentityResult.Success);
            _mockUserManager?.Setup(m => m.AddToRoleAsync(user, "Admin"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = _controller.RoleManagement(roleVM) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Null(user.CompanyId);
            _mockApplicationUserService.Verify(service => service.UpdateUser(user), Times.Once);
            _mockUserManager?.Verify(m => m.RemoveFromRoleAsync(user, SD.Role_Company), Times.Once);
            _mockUserManager?.Verify(m => m.AddToRoleAsync(user, "Admin"), Times.Once);
        }

        [Fact]
        public void RoleManagement_Post_WithNoRoleChange_DoesNotUpdateRoles()
        {
            // Arrange
            string userId = "user-123";
            var user = new ApplicationUser { Id = userId , Name = "User1" };
            var roleVM = new RoleManagemantVM
            {
                User = new ApplicationUser
                {
                    Id = userId,
                    Name = "User1",
                    Role = "Admin"
                }
            };

            _mockApplicationUserService.Setup(service => service.GetUserById(userId))
                .Returns(user);
            _mockUserManager?.Setup(m => m.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = _controller.RoleManagement(roleVM) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            _mockApplicationUserService.Verify(service => service.UpdateUser(It.IsAny<ApplicationUser>()), Times.Never);
            _mockUserManager?.Verify(m => m.RemoveFromRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
            _mockUserManager?.Verify(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
        }
    }
}