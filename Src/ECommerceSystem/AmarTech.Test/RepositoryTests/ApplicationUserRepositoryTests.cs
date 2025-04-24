using AmarTech.Infrastructure;
using AmarTech.Infrastructure.Repository;
using AmarTech.Domain.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AmarTech.Test.RepositoryTests
{
    public class ApplicationUserRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ApplicationUserRepositroy _repository;

        public ApplicationUserRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new ApplicationUserRepositroy(_context);

            // Setup initial data
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Add roles
            var roles = new[]
            {
                new IdentityRole { Id = "role1", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole { Id = "role2", Name = "Customer", NormalizedName = "CUSTOMER" }
            };
            _context.Roles.AddRange(roles);

            // Add users
            var users = new[]
            {
                new ApplicationUser
                {
                    Id = "user1",
                    UserName = "admin@example.com",
                    NormalizedUserName = "ADMIN@EXAMPLE.COM",
                    Email = "admin@example.com",
                    Name = "Admin User"
                },
                new ApplicationUser
                {
                    Id = "user2",
                    UserName = "customer@example.com",
                    NormalizedUserName = "CUSTOMER@EXAMPLE.COM",
                    Email = "customer@example.com",
                    Name = "Customer User"
                },
                new ApplicationUser
                {
                    Id = "user3",
                    UserName = "norole@example.com",
                    NormalizedUserName = "NOROLE@EXAMPLE.COM",
                    Email = "norole@example.com",
                    Name = "No Role User"
                }
            };
            _context.ApplicationUsers.AddRange(users);

            // Add user roles
            var userRoles = new[]
            {
                new IdentityUserRole<string> { UserId = "user1", RoleId = "role1" },
                new IdentityUserRole<string> { UserId = "user2", RoleId = "role2" }
            };
            _context.UserRoles.AddRange(userRoles);

            _context.SaveChanges();
        }

        [Fact]
        public void GetUserRole_ShouldReturnRoleName_WhenUserExists()
        {
            // Act
            var roleName = _repository.GetUserRole("user1");

            // Assert
            roleName.Should().Be("Admin");
        }

        [Fact]
        public void GetUserRole_ShouldReturnRoleName_ForCustomerUser()
        {
            // Act
            var roleName = _repository.GetUserRole("user2");

            // Assert
            roleName.Should().Be("Customer");
        }

        [Fact]
        public void GetUserRole_ShouldReturnEmptyString_WhenUserHasNoRole()
        {
            // Act
            var roleName = _repository.GetUserRole("user3");

            // Assert
            roleName.Should().BeEmpty();
        }

        [Fact]
        public void GetUserRole_ShouldReturnEmptyString_WhenUserIdIsNull()
        {
            // Act
            var roleName = _repository.GetUserRole(null!);

            // Assert
            roleName.Should().BeEmpty();
        }

        [Fact]
        public void GetUserRole_ShouldReturnEmptyString_WhenUserIdIsEmpty()
        {
            // Act
            var roleName = _repository.GetUserRole(string.Empty);

            // Assert
            roleName.Should().BeEmpty();
        }

        [Fact]
        public void GetUserRole_ShouldReturnEmptyString_WhenUserIdDoesNotExist()
        {
            // Act
            var roleName = _repository.GetUserRole("nonExistentUserId");

            // Assert
            roleName.Should().BeEmpty();
        }

        [Fact]
        public void Update_ShouldUpdateUser()
        {
            // Arrange
            var user = _context.ApplicationUsers.Find("user1");

            user!.Name = "Updated Admin Name";

            // Act
            _repository.Update(user);
            _context.SaveChanges();

            // Assert
            var updatedUser = _context.ApplicationUsers.Find("user1");
            updatedUser!.Name.Should().Be("Updated Admin Name");
        }

        [Fact]
        public void GetAllRoles_ShouldReturnAllRoles()
        {
            // Act
            var roles = _repository.GetAllRoles();

            // Assert
            roles.Should().HaveCount(2);
            roles.Should().Contain(r => r.Text == "Admin" && r.Value == "Admin");
            roles.Should().Contain(r => r.Text == "Customer" && r.Value == "Customer");
        }

        [Fact]
        public void Get_ShouldReturnUser_WhenUserExists()
        {
            // Act
            var user = _repository.Get(u => u.Id == "user1");

            // Assert
            user.Should().NotBeNull();
            user.UserName.Should().Be("admin@example.com");
            user.Name.Should().Be("Admin User");
        }

        [Fact]
        public void Get_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Act
            var user = _repository.Get(u => u.Id == "nonExistentUser");

            // Assert
            user.Should().BeNull();
        }

        [Fact]
        public void GetAll_ShouldReturnAllUsers()
        {
            // Act
            var users = _repository.GetAll();

            // Assert
            users.Should().HaveCount(3);
            users.Should().Contain(u => u.Id == "user1");
            users.Should().Contain(u => u.Id == "user2");
            users.Should().Contain(u => u.Id == "user3");
        }

        [Fact]
        public void GetAll_ShouldReturnFilteredUsers_WhenFilterIsApplied()
        {
            // Act
            var users = _repository.GetAll(u => u.Email!.Contains("admin"));

            // Assert
            users.Should().HaveCount(1);
            users.First().Id.Should().Be("user1");
        }

        [Fact]
        public void Add_ShouldAddNewUser()
        {
            // Arrange
            var newUser = new ApplicationUser
            {
                Id = "user4",
                UserName = "newuser@example.com",
                NormalizedUserName = "NEWUSER@EXAMPLE.COM",
                Email = "newuser@example.com",
                Name = "New User"
            };

            // Act
            _repository.Add(newUser);
            _context.SaveChanges();

            // Assert
            _context.ApplicationUsers.Find("user4").Should().NotBeNull();
            _context.ApplicationUsers.Count().Should().Be(4);
        }

        [Fact]
        public void Remove_ShouldRemoveUser()
        {
            // Arrange
            var user = _context.ApplicationUsers.Find("user3");

            // Act
            _repository.Remove(user!);
            _context.SaveChanges();

            // Assert
            _context.ApplicationUsers.Find("user3").Should().BeNull();
            _context.ApplicationUsers.Count().Should().Be(2);
        }

        [Fact]
        public void RemoveRange_ShouldRemoveMultipleUsers()
        {
            // Arrange
            var users = _context.ApplicationUsers.Where(u => u.Id == "user2" || u.Id == "user3").ToList();

            // Act
            _repository.RemoveRange(users);
            _context.SaveChanges();

            // Assert
            _context.ApplicationUsers.Count().Should().Be(1);
            _context.ApplicationUsers.Find("user1").Should().NotBeNull();
            _context.ApplicationUsers.Find("user2").Should().BeNull();
            _context.ApplicationUsers.Find("user3").Should().BeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}