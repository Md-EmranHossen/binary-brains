using AmarTech.Infrastructure;
using AmarTech.Infrastructure.Repository;
using AmarTech.Infrastructure.Repository.IRepository;
using AmarTech.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Linq;

namespace AmarTech.Test.RepositoryTests
{
    public class ProductRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ProductRepositroy _repository;

        public ProductRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _repository = new ProductRepositroy(_context);

            // Seed test data
            SeedTestData();
        }

        private void SeedTestData()
        {
            var products = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Title = "Product One",
                    Description = "Description One",
                    StockQuantity = 10,
                    CategoryId = 1,
                    CreatedBy = "TestUser",
                    CreatedDate = DateTime.Now.AddDays(-10),
                    Price = 19.99m
                },
                new Product
                {
                    Id = 2,
                    Title = "Product Two",
                    Description = "Description Two",
                    StockQuantity = 20,
                    CategoryId = 2,
                    CreatedBy = "TestUser",
                    CreatedDate = DateTime.Now.AddDays(-5),
                    Price = 29.99m
                },
                new Product
                {
                    Id = 3,
                    Title = "Unique Product",
                    Description = "Special Description",
                    StockQuantity = 5,
                    CategoryId = 1,
                    CreatedBy = "TestUser",
                    CreatedDate = DateTime.Now.AddDays(-2),
                    Price = 39.99m
                },
                new Product
                {
                    Id = 4,
                    Title = "Another Product",
                    Description = "Another Description",
                    StockQuantity = 15,
                    CategoryId = 3,
                    CreatedBy = "TestUser",
                    CreatedDate = DateTime.Now.AddDays(-1),
                    Price = 49.99m
                },
                new Product
                {
                    Id = 5,
                    Title = "Last Product",
                    Description = "Last Description",
                    StockQuantity = 25,
                    CategoryId = 2,
                    CreatedBy = "TestUser",
                    CreatedDate = DateTime.Now,
                    Price = 59.99m
                }
            };

            _context.Products.AddRange(products);

            // Add categories
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Electronics" },
                new Category { Id = 2, Name = "Clothing" },
                new Category { Id = 3, Name = "Books" }
            };

            _context.Categories.AddRange(categories);
            _context.SaveChanges();
        }

        [Fact]
        public async Task Update_UpdatesProduct_WhenProductExists()
        {
            // Arrange
            var product = await _context.Products.FindAsync(1);
            product.Should().NotBeNull();

            // Act
            product.Title = "Updated Product";
            _repository.Update(product);
            await _context.SaveChangesAsync();

            // Assert
            var updatedProduct = await _context.Products.FindAsync(1);
            updatedProduct.Should().NotBeNull();
            updatedProduct.Title.Should().Be("Updated Product");
        }

        [Fact]
        public void SkipAndTake_WithoutSearchAndInclude_ReturnsPaginatedResults()
        {
            // Arrange
            int pageSize = 2;
            int pageNumber = 1;

            // Act
            var result = _repository.SkipAndTake(pageSize, pageNumber);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(pageSize);
            result.First().Id.Should().Be(1); // First item on first page
        }

        [Fact]
        public void SkipAndTake_WithSecondPage_ReturnsCorrectItems()
        {
            // Arrange
            int pageSize = 2;
            int pageNumber = 2;

            // Act
            var result = _repository.SkipAndTake(pageSize, pageNumber);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(pageSize);
            result.First().Id.Should().Be(3); // First item on second page
        }

        [Fact]
        public void SkipAndTake_WithSearchQuery_ReturnsFilteredResults()
        {
            // Arrange
            int pageSize = 10;
            int pageNumber = 1;
            string searchQuery = "Unique";

            // Act
            var result = _repository.SkipAndTake(pageSize, pageNumber, searchQuery);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(1);
            result.First().Title.Should().Be("Unique Product");
        }

        [Fact]
        public void SkipAndTake_WithSearchInDescription_ReturnsFilteredResults()
        {
            // Arrange
            int pageSize = 10;
            int pageNumber = 1;
            string searchQuery = "Special";

            // Act
            var result = _repository.SkipAndTake(pageSize, pageNumber, searchQuery);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(1);
            result.First().Description.Should().Be("Special Description");
        }

        [Fact]
        public void SkipAndTake_WithIncludeProperties_ReturnsResultsWithIncludedEntities()
        {
            // Arrange
            int pageSize = 10;
            int pageNumber = 1;
            string includeProperties = "Category"; // Assuming there's a Category navigation property in Product

            // Act
            var result = _repository.SkipAndTake(pageSize, pageNumber, null, includeProperties);

            // Assert
            result.Should().NotBeNull();
            result.Count().Should().Be(5);
        }


        [Fact]
        public void ReduceStockCount_DecreasesStockQuantity_ForEachProductInCart()
        {
            // Arrange
            var cartList = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 1, Count = 2 },
                new ShoppingCart { ProductId = 2, Count = 3 }
            };

            var initialStock1 = _context?.Products?.Find(1)?.StockQuantity;
            var initialStock2 = _context?.Products?.Find(2)?.StockQuantity;

            // Act
            _repository.ReduceStockCount(cartList);
            _context.SaveChanges();

            // Assert
            _context?.Products?.Find(1)?.StockQuantity.Should().Be(initialStock1 - 2);
            _context?.Products?.Find(2)?.StockQuantity.Should().Be(initialStock2 - 3);
        }

        [Fact]
        public void ReduceStockCount_HandlesProductNotFound_WithoutErrors()
        {
            // Arrange
            var cartList = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 999, Count = 2 } // Non-existent product ID
            };

            // Act - shouldn't throw exception
            Action act = () => _repository.ReduceStockCount(cartList);

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void GetAllProductsCount_WithoutSearchQuery_ReturnsCorrectCount()
        {
            // Act
            var count = _repository.GetAllProductsCount();

            // Assert
            count.Should().Be(5); // Total number of products seeded
        }

        [Fact]
        public void GetAllProductsCount_WithSearchQuery_ReturnsFilteredCount()
        {
            // Arrange
            string searchQuery = "Unique";

            // Act
            var count = _repository.GetAllProductsCount(searchQuery);

            // Assert
            count.Should().Be(1); // Only one product with "Unique" in title
        }

        [Fact]
        public void GetAllProductsCount_WithSearchQueryInDescription_ReturnsFilteredCount()
        {
            // Arrange
            string searchQuery = "Special";

            // Act
            var count = _repository.GetAllProductsCount(searchQuery);

            // Assert
            count.Should().Be(1); // Only one product with "Special" in description
        }

        [Fact]
        public void GetAllProductsCount_WithCaseInsensitiveSearch_ReturnsCorrectResults()
        {
            // Arrange
            string searchQuery = "unique"; // lowercase, original is "Unique"

            // Act
            var count = _repository.GetAllProductsCount(searchQuery);

            // Assert
            count.Should().Be(1); // Should find regardless of case
        }

        [Fact]
        public void GetAllProductsCount_WithEmptySearchQuery_ReturnsAllProducts()
        {
            // Arrange
            string searchQuery = "";

            // Act
            var count = _repository.GetAllProductsCount(searchQuery);

            // Assert
            count.Should().Be(5); // Should return all products
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}