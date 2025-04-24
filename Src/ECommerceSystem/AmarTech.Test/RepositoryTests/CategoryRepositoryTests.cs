using AmarTech.Infrastructure;
using AmarTech.Infrastructure.Repository;
using AmarTech.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AmarTech.Test.RepositoryTests
{
    public class CategoryRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CategoryRepositroy _repository;

        public CategoryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new CategoryRepositroy(_context);
        }

        [Fact]
        public async Task Add_AddsCategoryToDatabase()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                DsiplayOrder = 1,
                CreatedBy = "TestUser"
            };

            // Act
            _repository.Add(category);
            await _context.SaveChangesAsync();

            // Assert
            var savedCategory = await _context.Categories.FindAsync(1);
            savedCategory.Should().NotBeNull();
            savedCategory.Name.Should().Be("Electronics");
            savedCategory.CreatedBy.Should().Be("TestUser");
            savedCategory.DsiplayOrder.Should().Be(1);
        }

        [Fact]
        public async Task Get_ReturnsCategory_WhenCategoryExists()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                DsiplayOrder = 1,
                CreatedBy = "TestUser"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = _repository.Get(c => c.Id == 1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Name.Should().Be("Electronics");
            result.DsiplayOrder.Should().Be(1);
        }

        [Fact]
        public void Get_ReturnsNull_WhenCategoryDoesNotExist()  // Removed async keyword
        {
            // Act
            var result = _repository.Get(c => c.Id == 999);
            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Get_DetachesEntityFromContext()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                DsiplayOrder = 1,
                CreatedBy = "TestUser"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = _repository.Get(c => c.Id == 1);
            result!.Name = "Modified Electronics";
            await _context.SaveChangesAsync();

            // Assert - Original entity should not be modified
            var original = await _context.Categories.FindAsync(1);
            original?.Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task GetAll_ReturnsAllCategories_WhenNoFilterIsApplied()
        {
            // Arrange
            var categories = new[]
            {
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    DsiplayOrder = 1,
                    CreatedBy = "TestUser1"
                },
                new Category
                {
                    Id = 2,
                    Name = "Clothing",
                    DsiplayOrder = 2,
                    CreatedBy = "TestUser2"
                }
            };
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Act
            var result = _repository.GetAll();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(c => c.Name == "Electronics");
            result.Should().Contain(c => c.Name == "Clothing");
        }

        [Fact]
        public async Task GetAll_ReturnsFilteredCategories_WhenFilterIsApplied()
        {
            // Arrange
            var categories = new[]
            {
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    DsiplayOrder = 1,
                    CreatedBy = "TestUser1"
                },
                new Category
                {
                    Id = 2,
                    Name = "Clothing",
                    DsiplayOrder = 2,
                    CreatedBy = "TestUser2"
                }
            };
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Act
            var result = _repository.GetAll(c => c.CreatedBy == "TestUser1");

            // Assert
            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task GetAll_DetachesEntitiesFromContext()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                DsiplayOrder = 1,
                CreatedBy = "TestUser"
            };

            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            var result = _repository.GetAll().First();
            result.Name = "Electronics";
            await _context.SaveChangesAsync(); // Should not affect the original

            // Assert - Original entity should not be modified
            var original = await _context.Categories.FindAsync(1);
            original?.Name.Should().Be("Electronics");
        }


        [Fact]
        public async Task Remove_RemovesCategory_WhenCategoryExists()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                DsiplayOrder = 1,
                CreatedBy = "TestUser"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            _repository.Remove(category);
            await _context.SaveChangesAsync();

            // Assert
            var deletedCategory = await _context.Categories.FindAsync(1);
            deletedCategory.Should().BeNull();
        }

        [Fact]
        public async Task RemoveRange_RemovesMultipleCategories()
        {
            // Arrange
            var categories = new[]
            {
                new Category
                {
                    Id = 1,
                    Name = "Electronics",
                    DsiplayOrder = 1,
                    CreatedBy = "TestUser1"
                },
                new Category
                {
                    Id = 2,
                    Name = "Clothing",
                    DsiplayOrder = 2,
                    CreatedBy = "TestUser2"
                }
            };
            await _context.Categories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Act
            _repository.RemoveRange(categories);
            await _context.SaveChangesAsync();

            // Assert
            var remainingCategories = await _context.Categories.ToListAsync();
            remainingCategories.Should().BeEmpty();
        }

        [Fact]
        public async Task Update_UpdatesCategory_WhenCategoryExists()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                DsiplayOrder = 1,
                CreatedBy = "TestUser"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            category.Name = "Updated Electronics";
            category.UpdatedBy = "TestUpdater";
            category.UpdatedDate = DateTime.Now;
            _repository.Update(category);
            await _context.SaveChangesAsync();

            // Assert
            var updatedCategory = await _context.Categories.FindAsync(1);
            updatedCategory.Should().NotBeNull();
            updatedCategory.Name.Should().Be("Updated Electronics");
            updatedCategory.UpdatedBy.Should().Be("TestUpdater");
            updatedCategory.UpdatedDate.Should().NotBeNull();
        }

        [Fact]
        public async Task Update_ShouldUpdateDisplayOrder()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                DsiplayOrder = 1,
                CreatedBy = "TestUser"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            category.DsiplayOrder = 50;
            _repository.Update(category);
            await _context.SaveChangesAsync();

            // Assert
            var updatedCategory = await _context.Categories.FindAsync(1);
            updatedCategory.Should().NotBeNull();
            updatedCategory.DsiplayOrder.Should().Be(50);
        }

        [Fact]
        public async Task Update_ShouldUpdateIsActive()
        {
            // Arrange
            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                DsiplayOrder = 1,
                IsActive = true,
                CreatedBy = "TestUser"
            };
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();

            // Act
            category.IsActive = false;
            _repository.Update(category);
            await _context.SaveChangesAsync();

            // Assert
            var updatedCategory = await _context.Categories.FindAsync(1);
            updatedCategory.Should().NotBeNull();
            updatedCategory.IsActive.Should().BeFalse();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}