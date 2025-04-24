using AmarTech.Infrastructure;
using AmarTech.Infrastructure.Repository;
using AmarTech.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ECommerceSystem.Test.RepositoryTests
{
    public class OrderDetailRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly OrderDetailRepositroy _repository;

        public OrderDetailRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new OrderDetailRepositroy(_context);
        }

        [Fact]
        public async Task Update_UpdatesOrderDetail_WhenOrderDetailExists()
        {
            // Arrange
            var orderDetail = new OrderDetail
            {
                Id = 1,
                ProductId = 201,
                Count = 2,
                Price = 19.99
            };
            await _context.OrderDetails.AddAsync(orderDetail);
            await _context.SaveChangesAsync();

            // Act
            orderDetail.Count = 5;
            orderDetail.Price = 24.99;
            _repository.Update(orderDetail);
            await _context.SaveChangesAsync();

            // Assert
            var updatedOrderDetail = await _context.OrderDetails.FindAsync(1);
            updatedOrderDetail.Should().NotBeNull();
            updatedOrderDetail.Count.Should().Be(5);
            updatedOrderDetail.Price.Should().Be(24.99);
            updatedOrderDetail.ProductId.Should().Be(201); // Unchanged value
        }

        [Fact]
        public async Task Update_ShouldNotUpdateOtherOrderDetails()
        {
            // Arrange
            var orderDetail1 = new OrderDetail
            {
                Id = 1,
                ProductId = 201,
                Count = 2,
                Price = 19.99
            };

            var orderDetail2 = new OrderDetail
            {
                Id = 2,
                ProductId = 202,
                Count = 3,
                Price = 29.99
            };

            await _context.OrderDetails.AddRangeAsync(orderDetail1, orderDetail2);
            await _context.SaveChangesAsync();

            // Act
            orderDetail1.Count = 5;
            orderDetail1.Price = 24.99;
            _repository.Update(orderDetail1);
            await _context.SaveChangesAsync();

            // Assert
            var updatedOrderDetail1 = await _context.OrderDetails.FindAsync(1);
            var unchangedOrderDetail2 = await _context.OrderDetails.FindAsync(2);

            updatedOrderDetail1?.Count.Should().Be(5);
            updatedOrderDetail1?.Price.Should().Be(24.99);

            unchangedOrderDetail2?.Count.Should().Be(3); // Should remain unchanged
            unchangedOrderDetail2?.Price.Should().Be(29.99); // Should remain unchanged
        }

        [Fact]
        public async Task Update_ShouldUpdateAllProvidedProperties()
        {
            // Arrange
            var orderDetail = new OrderDetail
            {
                Id = 1,
                ProductId = 201,
                Count = 2,
                Price = 19.99
            };
            await _context.OrderDetails.AddAsync(orderDetail);
            await _context.SaveChangesAsync();

            // Act
            orderDetail.ProductId = 205;
            orderDetail.Count = 10;
            orderDetail.Price = 99.99;

            _repository.Update(orderDetail);
            await _context.SaveChangesAsync();

            // Assert
            var updatedOrderDetail = await _context.OrderDetails.FindAsync(1);
            updatedOrderDetail.Should().NotBeNull();
            updatedOrderDetail.ProductId.Should().Be(205);
            updatedOrderDetail.Count.Should().Be(10);
            updatedOrderDetail.Price.Should().Be(99.99);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}