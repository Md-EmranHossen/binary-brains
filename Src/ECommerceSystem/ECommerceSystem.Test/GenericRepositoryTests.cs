using ECommerceSystem.DataAccess;
using ECommerceSystem.DataAccess.Repository;
using ECommerceSystem.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ECommerceSystem.Test
{
    public class GenericRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Repository<Product> _repository;

        public GenericRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging() // ডিবাগিংয়ের জন্য
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new Repository<Product>(_context);
        }

        [Fact]
        public async Task Add_AddsProductToDatabase()
        {
            var product = new Product
            {
                Id = 1,
                Title = "Test Product",
                CategoryId = 1,
                CreatedBy = "TestUser", // Required property
                Description = "Test Description" // Required property
            };

            _repository.Add(product);
            await _context.SaveChangesAsync();

            var savedProduct = await _context.Products.FindAsync(1);
            savedProduct.Should().NotBeNull();
            savedProduct.Title.Should().Be("Test Product");
            savedProduct.CategoryId.Should().Be(1);
        }

        [Fact]
        public async Task Get_ReturnsProduct_WhenProductExists()
        {
            var product = new Product
            {
                Id = 1,
                Title = "Test Product",
                CategoryId = 1,
                CreatedBy = "TestUser",
                Description = "Test Description"
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var result = _repository.Get(p => p.Id == 1);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Title.Should().Be("Test Product");
        }

        [Fact]
        public async Task Get_ReturnsNull_WhenProductDoesNotExist()
        {
            var result = _repository.Get(p => p.Id == 999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task Get_IncludesCategory_WhenIncludePropertiesSpecified()
        {
            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                CreatedBy = "Emran" // Category এর required property
            };
            var product = new Product
            {
                Id = 1,
                Title = "Test Product",
                CategoryId = 1,
                Category = category,
                CreatedBy = "TestUser",
                Description = "Test Description"
            };
            await _context.Categories.AddAsync(category);
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var result = _repository.Get(p => p.Id == 1, includeProperties: "Category");

            result.Should().NotBeNull();
            result.Category.Should().NotBeNull();
            result.Category.Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task Get_UsesNoTracking_WhenTrackedIsFalse()
        {
            var product = new Product
            {
                Id = 1,
                Title = "Test Product",
                CategoryId = 1,
                CreatedBy = "TestUser",
                Description = "Test Description"
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var result = _repository.Get(p => p.Id == 1, tracked: false);
            result.Title = "Modified";

            var original = await _context.Products.FindAsync(1);
            original.Title.Should().Be("Test Product");
        }

        [Fact]
        public async Task GetAll_ReturnsAllProducts_WhenNoFilterIsApplied()
        {
            var products = new[]
            {
                new Product
                {
                    Id = 1,
                    Title = "Product 1",
                    CategoryId = 1,
                    CreatedBy = "TestUser",
                    Description = "Test Description 1"
                },
                new Product
                {
                    Id = 2,
                    Title = "Product 2",
                    CategoryId = 1,
                    CreatedBy = "TestUser",
                    Description = "Test Description 2"
                }
            };
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            var result = _repository.GetAll();

            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Title == "Product 1");
            result.Should().Contain(p => p.Title == "Product 2");
        }

        [Fact]
        public async Task GetAll_ReturnsFilteredProducts_WhenFilterIsApplied()
        {
            var products = new[]
            {
                new Product
                {
                    Id = 1,
                    Title = "Product 1",
                    CategoryId = 1,
                    CreatedBy = "TestUser",
                    Description = "Test Description 1"
                },
                new Product
                {
                    Id = 2,
                    Title = "Product 2",
                    CategoryId = 1,
                    CreatedBy = "TestUser",
                    Description = "Test Description 2"
                }
            };
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            var result = _repository.GetAll(p => p.Id == 1);

            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Product 1");
        }

        [Fact]
        public async Task GetAll_IncludesCategory_WhenIncludePropertiesSpecified()
        {
            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                CreatedBy = "Emran"
            };
            var products = new[]
            {
                new Product
                {
                    Id = 1,
                    Title = "Product 1",
                    CategoryId = 1,
                    Category = category,
                    CreatedBy = "TestUser",
                    Description = "Test Description 1"
                },
                new Product
                {
                    Id = 2,
                    Title = "Product 2",
                    CategoryId = 1,
                    Category = category,
                    CreatedBy = "TestUser",
                    Description = "Test Description 2"
                }
            };
            await _context.Categories.AddAsync(category);
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            var result = _repository.GetAll(includeProperties: "Category");

            result.Should().HaveCount(2);
            result.First().Category.Should().NotBeNull();
            result.First().Category.Name.Should().Be("Electronics");
        }

        [Fact]
        public async Task Remove_RemovesProduct_WhenProductExists()
        {
            var product = new Product
            {
                Id = 1,
                Title = "Test Product",
                CategoryId = 1,
                CreatedBy = "TestUser",
                Description = "Test Description"
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            _repository.Remove(product);
            await _context.SaveChangesAsync();

            var deletedProduct = await _context.Products.FindAsync(1);
            deletedProduct.Should().BeNull();
        }

        [Fact]
        public async Task RemoveRange_RemovesMultipleProducts()
        {
            var products = new[]
            {
                new Product
                {
                    Id = 1,
                    Title = "Product 1",
                    CategoryId = 1,
                    CreatedBy = "TestUser",
                    Description = "Test Description 1"
                },
                new Product
                {
                    Id = 2,
                    Title = "Product 2",
                    CategoryId = 1,
                    CreatedBy = "TestUser",
                    Description = "Test Description 2"
                }
            };
            await _context.Products.AddRangeAsync(products);
            await _context.SaveChangesAsync();

            _repository.RemoveRange(products);
            await _context.SaveChangesAsync();

            var remainingProducts = await _context.Products.ToListAsync();
            remainingProducts.Should().BeEmpty();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}