using AmarTech.Infrastructure;
using AmarTech.Infrastructure.Repository;
using AmarTech.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        [Fact]
        public void CombineToDB_UpdatesExistingCartAndAddsNewItems()
        {
            // Arrange
            const string userId = "user1";

            // Create carts in DB
            var cartsInDb = new List<ShoppingCart>
            {
                new ShoppingCart { Id = 1, ApplicationUserId = userId, ProductId = 101, Count = 2 },
                new ShoppingCart { Id = 2, ApplicationUserId = userId, ProductId = 102, Count = 1 }
            };
            _context.ShoppingCarts.AddRange(cartsInDb);
            _context.SaveChanges();

            // Create carts in memory (some overlap with DB, some new)
            var cartsInMemory = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 101, Count = 3 }, // Matches existing DB item
                new ShoppingCart { ProductId = 103, Count = 4 }  // New item
            };

            // Act
            _repository.CombineToDB(cartsInDb, cartsInMemory, userId);

            // Assert
            // Check updated existing cart
            var updatedCart = _context.ShoppingCarts.Find(1);
            updatedCart.Should().NotBeNull();
            updatedCart.Count.Should().Be(5); // 2 + 3

            // Check untouched cart
            var unchangedCart = _context.ShoppingCarts.Find(2);
            unchangedCart.Should().NotBeNull();
            unchangedCart.Count.Should().Be(1);

            // Check newly added cart
            var newCart = _context.ShoppingCarts.FirstOrDefault(c => c.ProductId == 103);
            newCart.Should().NotBeNull();
            newCart.Count.Should().Be(4);
            newCart.ApplicationUserId.Should().Be(userId);
        }

        [Fact]
        public void CombineToDB_SkipsNonMatchingDbItems()
        {
            // Arrange
            const string userId = "user1";

            // Create carts in DB
            var cartsInDb = new List<ShoppingCart>
            {
                new ShoppingCart { Id = 1, ApplicationUserId = userId, ProductId = 101, Count = 2 },
                new ShoppingCart { Id = 2, ApplicationUserId = userId, ProductId = 102, Count = 1 }
            };
            _context.ShoppingCarts.AddRange(cartsInDb);
            _context.SaveChanges();

            // Create memory cart with no matching DB items
            var cartsInMemory = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 103, Count = 4 },
                new ShoppingCart { ProductId = 104, Count = 5 }
            };

            // Act
            _repository.CombineToDB(cartsInDb, cartsInMemory, userId);

            // Assert
            // Check DB items remain unchanged
            var dbCart1 = _context.ShoppingCarts.Find(1);
            dbCart1.Should().NotBeNull();
            dbCart1.Count.Should().Be(2);

            var dbCart2 = _context.ShoppingCarts.Find(2);
            dbCart2.Should().NotBeNull();
            dbCart2.Count.Should().Be(1);

            // Check new items were added
            var newCarts = _context.ShoppingCarts.Where(c => c.ProductId == 103 || c.ProductId == 104).ToList();
            newCarts.Should().HaveCount(2);
            newCarts.Should().Contain(c => c.ProductId == 103 && c.Count == 4);
            newCarts.Should().Contain(c => c.ProductId == 104 && c.Count == 5);
        }

        [Fact]
        public void CombineToDB_HandlesEmptyMemoryCart()
        {
            // Arrange
            const string userId = "user1";

            // Create carts in DB
            var cartsInDb = new List<ShoppingCart>
            {
                new ShoppingCart { Id = 1, ApplicationUserId = userId, ProductId = 101, Count = 2 },
                new ShoppingCart { Id = 2, ApplicationUserId = userId, ProductId = 102, Count = 1 }
            };
            _context.ShoppingCarts.AddRange(cartsInDb);
            _context.SaveChanges();

            // Empty memory cart
            var cartsInMemory = new List<ShoppingCart>();

            // Act
            _repository.CombineToDB(cartsInDb, cartsInMemory, userId);

            // Assert
            // Check DB items remain unchanged
            var dbCarts = _context.ShoppingCarts.ToList();
            dbCarts.Should().HaveCount(2);
            dbCarts.Should().Contain(c => c.Id == 1 && c.Count == 2);
            dbCarts.Should().Contain(c => c.Id == 2 && c.Count == 1);
        }

        [Fact]
        public void CombineToDB_HandlesEmptyDbCart()
        {
            // Arrange
            const string userId = "user1";

            // Empty DB cart
            var cartsInDb = new List<ShoppingCart>();

            // Create memory carts
            var cartsInMemory = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 103, Count = 4 },
                new ShoppingCart { ProductId = 104, Count = 5 }
            };

            // Act
            _repository.CombineToDB(cartsInDb, cartsInMemory, userId);

            // Assert
            // Check new items were added
            var dbCarts = _context.ShoppingCarts.ToList();
            dbCarts.Should().HaveCount(2);
            dbCarts.Should().Contain(c => c.ProductId == 103 && c.Count == 4);
            dbCarts.Should().Contain(c => c.ProductId == 104 && c.Count == 5);
            dbCarts.All(c => c.ApplicationUserId == userId).Should().BeTrue();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}