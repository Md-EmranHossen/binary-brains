using AmarTech.Infrastructure;
using AmarTech.Infrastructure.Repository;
using FluentAssertions;
using AmarTech.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AmarTech.Test.RepositoryTests
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly UnitOfWork _unitOfWork;

        public UnitOfWorkTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            _context = new ApplicationDbContext(options);
            _unitOfWork = new UnitOfWork(_context);
        }

        [Fact]
        public void Constructor_InitializesCorrectly()
        {
            // Assert
            _unitOfWork.Should().NotBeNull();
        }

        [Fact]
        public void Commit_SavesChangesToDatabase()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Title = "Test Product",
                Description = "Test Description",
                CategoryId = 1,
                CreatedBy = "TestUser"
            };
            _context.Products.Add(product);

            // Act
            _unitOfWork.Commit();

            // Assert
            var savedProduct = _context.Products.Find(1);
            savedProduct.Should().NotBeNull();
            savedProduct.Title.Should().Be("Test Product");
        }

        [Fact]
        public async Task Commit_PersistsMultipleChanges()
        {
            // Arrange
            var product1 = new Product
            {
                Id = 1,
                Title = "Product 1",
                Description = "Description 1",
                CategoryId = 1,
                CreatedBy = "TestUser"
            };

            var product2 = new Product
            {
                Id = 2,
                Title = "Product 2",
                Description = "Description 2",
                CategoryId = 1,
                CreatedBy = "TestUser"
            };

            _context.Products.AddRange(product1, product2);

            // Act
            _unitOfWork.Commit();

            // Assert
            var products = await _context.Products.ToListAsync();
            products.Should().HaveCount(2);
            products.Should().Contain(p => p.Id == 1);
            products.Should().Contain(p => p.Id == 2);
        }

        [Fact]
        public void Commit_PersistsUpdates()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Title = "Original Title",
                Description = "Test Description",
                CategoryId = 1,
                CreatedBy = "TestUser"
            };
            _context.Products.Add(product);
            _unitOfWork.Commit();

            // Update the product
            product.Title = "Updated Title";
            _context.Products.Update(product);

            // Act
            _unitOfWork.Commit();

            // Assert
            var updatedProduct = _context.Products.Find(1);
            updatedProduct.Should().NotBeNull();
            updatedProduct.Title.Should().Be("Updated Title");
        }

        [Fact]
        public void Commit_PersistsDeletes()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Title = "Test Product",
                Description = "Test Description",
                CategoryId = 1,
                CreatedBy = "TestUser"
            };
            _context.Products.Add(product);
            _unitOfWork.Commit();

            // Delete the product
            _context.Products.Remove(product);

            // Act
            _unitOfWork.Commit();

            // Assert
            var deletedProduct = _context.Products.Find(1);
            deletedProduct.Should().BeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}