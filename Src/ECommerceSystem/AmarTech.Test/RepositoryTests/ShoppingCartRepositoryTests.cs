using AmarTech.Infrastructure;
using AmarTech.Infrastructure.Repository;
using AmarTech.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AmarTech.Test.RepositoryTests
{
    public class ShoppingCartRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ShoppingCartRepository _repository;

        public ShoppingCartRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new ShoppingCartRepository(_context);
        }

        [Fact]
        public async Task Update_UpdatesShoppingCart_WhenCartExists()
        {
            // Arrange
            var shoppingCart = new ShoppingCart
            {
                Id = 1,
                ApplicationUserId = "user1",
                ProductId = 101,
                Count = 2
            };
            await _context.ShoppingCarts.AddAsync(shoppingCart);
            await _context.SaveChangesAsync();

            // Act
            shoppingCart.Count = 5;
            _repository.Update(shoppingCart);
            await _context.SaveChangesAsync();

            // Assert
            var updatedCart = await _context.ShoppingCarts.FindAsync(1);
            updatedCart.Should().NotBeNull();
            updatedCart.Count.Should().Be(5);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}