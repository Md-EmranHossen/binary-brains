using AmarTech.Domain.Entities;
using AmarTech.Infrastructure.Repository.IRepository;
using AmarTech.Application;
using AmarTech.Application.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AmarTech.Test.ServiceTests
{
    public class ApplicationUserServiceTests
    {
        private readonly Mock<IApplicationUserRepository> _mockApplicationUserRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly ApplicationUserService _applicationUserService;

        public ApplicationUserServiceTests()
        {
            _mockApplicationUserRepository = new Mock<IApplicationUserRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _applicationUserService = new ApplicationUserService(_mockApplicationUserRepository.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public void GetAllRoles_ShouldReturnRoles()
        {
            // Arrange
            var expectedRoles = new List<SelectListItem>
            {
                new SelectListItem { Text = "Admin", Value = "Admin" },
                new SelectListItem { Text = "User", Value = "User" }
            };

            _mockApplicationUserRepository.Setup(repo => repo.GetAllRoles())
                .Returns(expectedRoles);

            // Act
            var result = _applicationUserService.GetAllRoles();

            // Assert
            Assert.Equal(expectedRoles, result);
            _mockApplicationUserRepository.Verify(repo => repo.GetAllRoles(), Times.Once);
        }

        [Fact]
        public void GetAllUsers_ShouldReturnUsersWithCompanyProperty()
        {
            // Arrange
            var expectedUsers = new List<ApplicationUser>
            {
                new ApplicationUser { Id = "1", Name = "User1", Email = "user1@example.com", Company = new Company { Id = 1, Name = "Company1" } },
                new ApplicationUser { Id = "2", Name = "User2", Email = "user2@example.com", Company = new Company { Id = 2, Name = "Company2" } }
            };

            _mockApplicationUserRepository.Setup(repo => repo.GetAll(null, "Company"))
                .Returns(expectedUsers);

            // Act
            var result = _applicationUserService.GetAllUsers();

            // Assert
            Assert.Equal(expectedUsers, result);
            _mockApplicationUserRepository.Verify(repo => repo.GetAll(null, "Company"), Times.Once);
        }

        [Fact]
        public void GetAllUsersCount_ShouldReturnTotalUserCount()
        {
            // Arrange
            int expectedCount = 5;

            _mockApplicationUserRepository.Setup(repo => repo.GetAllUsersCount())
                .Returns(expectedCount);

            // Act
            var result = _applicationUserService.GetAllUsersCount();

            // Assert
            Assert.Equal(expectedCount, result);
            _mockApplicationUserRepository.Verify(repo => repo.GetAllUsersCount(), Times.Once);
        }

        [Fact]
        public void GetUserRole_ShouldReturnUserRole()
        {
            // Arrange
            string userId = "1";
            string expectedRole = "Admin";

            _mockApplicationUserRepository.Setup(repo => repo.GetUserRole(userId))
                .Returns(expectedRole);

            // Act
            var result = _applicationUserService.GetUserrole(userId);

            // Assert
            Assert.Equal(expectedRole, result);
            _mockApplicationUserRepository.Verify(repo => repo.GetUserRole(userId), Times.Once);
        }

        [Fact]
        public void UpdateUser_ShouldUpdateUserAndCommit()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1", Name = "UpdatedName", Email = "updated@example.com" };

            // Act
            _applicationUserService.UpdateUser(user);

            // Assert
            _mockApplicationUserRepository.Verify(repo => repo.Update(user), Times.Once);
            _mockUnitOfWork.Verify(uow => uow.Commit(), Times.Once);
        }

        [Fact]
        public void GetUserById_ShouldReturnUserWithMatchingId()
        {
            // Arrange
            string userId = "1";
            var expectedUser = new ApplicationUser { Id = userId, Name = "User1", Email = "user1@example.com" };

            _mockApplicationUserRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null, false))
                .Returns(expectedUser);

            // Act
            var result = _applicationUserService.GetUserById(userId);

            // Assert
            Assert.Equal(expectedUser, result);
            _mockApplicationUserRepository.Verify(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void GetUserByIdAndIncludeProp_ShouldReturnUserWithMatchingIdAndIncludeProperty()
        {
            // Arrange
            string userId = "1";
            string includeProperty = "Company";
            var expectedUser = new ApplicationUser
            {
                Id = userId,
                Name = "User1",
                Email = "user1@example.com",
                Company = new Company { Id = 1, Name = "Company1" }
            };

            _mockApplicationUserRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), includeProperty, false))
                .Returns(expectedUser);

            // Act
            var result = _applicationUserService.GetUserByIdAndIncludeprop(userId, includeProperty);

            // Assert
            Assert.Equal(expectedUser, result);
            _mockApplicationUserRepository.Verify(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), includeProperty, false), Times.Once);
        }

        [Fact]
        public void GetUserById_ShouldReturnNullWhenUserNotFound()
        {
            // Arrange
            string userId = "nonexistent";

            _mockApplicationUserRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null, false))
                .Returns((ApplicationUser?)null);

            // Act
            var result = _applicationUserService.GetUserById(userId);

            // Assert
            Assert.Null(result);
            _mockApplicationUserRepository.Verify(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void GetUserByIdAndIncludeProp_ShouldReturnNullWhenUserNotFound()
        {
            // Arrange
            string userId = "nonexistent";
            string includeProperty = "Company";

            _mockApplicationUserRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), includeProperty, false))
                .Returns((ApplicationUser?)null);

            // Act
            var result = _applicationUserService.GetUserByIdAndIncludeprop(userId, includeProperty);

            // Assert
            Assert.Null(result);
            _mockApplicationUserRepository.Verify(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), includeProperty, false), Times.Once);
        }

        [Fact]
        public void GetUserName_ShouldReturnUserNameWhenUserExists()
        {
            // Arrange
            string userId = "1";
            var expectedUser = new ApplicationUser { Id = userId, Name = "User1" };

            _mockApplicationUserRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null, false))
                .Returns(expectedUser);

            // Act
            var result = _applicationUserService.GetUserName(userId);

            // Assert
            Assert.Equal("User1", result);
            _mockApplicationUserRepository.Verify(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void GetUserName_ShouldReturnEmptyStringWhenUserDoesNotExist()
        {
            // Arrange
            string userId = "nonexistent";

            _mockApplicationUserRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null, false))
                .Returns((ApplicationUser?)null);

            // Act
            var result = _applicationUserService.GetUserName(userId);

            // Assert
            Assert.Equal("", result);
            _mockApplicationUserRepository.Verify(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void GetUserName_ShouldReturnEmptyStringWhenUserNameIsNull()
        {
            // Arrange
            string userId = "1";
            var expectedUser = new ApplicationUser { Id = userId, Name = null };

            _mockApplicationUserRepository.Setup(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null, false))
                .Returns(expectedUser);

            // Act
            var result = _applicationUserService.GetUserName(userId);

            // Assert
            Assert.Equal("", result);
            _mockApplicationUserRepository.Verify(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void GetUserName_ShouldReturnEmptyStringWhenUserIdIsNull()
        {
            // Arrange
            string? userId = null;

            // Act
            var result = _applicationUserService.GetUserName(userId);

            // Assert
            Assert.Equal("", result);
            _mockApplicationUserRepository.Verify(repo => repo.Get(It.IsAny<Expression<Func<ApplicationUser, bool>>>(), null, false), Times.Once);
        }
    }
}