using AmarTech.Infrastructure;
using AmarTech.Infrastructure.Repository;
using AmarTech.Infrastructure.Repository.IRepository;
using AmarTech.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ECommerceSystem.Test.RepositoryTests
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
        }
        [Fact]
        public async Task Update_UpdatesProduct_WhenProductExists()
        {
            var product = new Product
            {
                Id = 1,
               Title = "Test Product",
                CategoryId = 1,
                CreatedBy = "TestUser", // Required property
                Description = "Test Description" // Required property
            };
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            product.Title = "Updated Product";
            _repository.Update(product);
            await _context.SaveChangesAsync();

            var updatedProduct = await _context.Products.FindAsync(1);
            updatedProduct?.Title.Should().Be("Updated Product");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}