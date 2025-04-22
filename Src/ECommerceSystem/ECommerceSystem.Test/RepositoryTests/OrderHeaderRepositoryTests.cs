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
    public class OrderHeaderRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly OrderHeaderRepositroy _repository;

        public OrderHeaderRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new OrderHeaderRepositroy(_context);

            // Setup initial test data
            SeedDatabase();
        }

        private void SeedDatabase()
        {
            // Add sample order headers
            var orderHeaders = new[]
            {
                new OrderHeader
                {
                    Id = 1,
                    ApplicationUserId = "user1",
                    OrderDate = DateTime.Now.AddDays(-10),
                    ShippingDate = DateTime.Now.AddDays(-5),
                    OrderTotal = 199.99,
                    OrderStatus = "Pending",
                    PaymentStatus = "Pending",
                    TrackingNumber = "TRK123456",
                    Carrier = "UPS",
                    PaymentDate = default, // Using default(DateTime) for non-nullable DateTime
                    PaymentIntentId = null,
                    SessionId = null,
                    PhoneNumber = "1234567890",
                    StreetAddress = "123 Main St",
                    City = "Anytown",
                    State = "CA",
                    PostalCode = "12345",
                    Name = "John Doe"
                },
                new OrderHeader
                {
                    Id = 2,
                    ApplicationUserId = "user2",
                    OrderDate = DateTime.Now.AddDays(-5),
                    ShippingDate = DateTime.Now,
                    OrderTotal = 99.99,
                    OrderStatus = "Processing",
                    PaymentStatus = "Approved",
                    TrackingNumber = "TRK789012",
                    Carrier = "FedEx",
                    PaymentDate = DateTime.Now.AddDays(-4),
                    PaymentIntentId = "pi_123456",
                    SessionId = "cs_123456",
                    PhoneNumber = "0987654321",
                    StreetAddress = "456 Elm St",
                    City = "Othertown",
                    State = "NY",
                    PostalCode = "67890",
                    Name = "Jane Smith"
                }
            };

            _context.OrderHeaders.AddRange(orderHeaders);
            _context.SaveChanges();
        }

        [Fact]
        public void Update_ShouldUpdateOrderHeader()
        {
            // Arrange
            var order = _context.OrderHeaders.Find(1);
            order.TrackingNumber = "TRK999999";
            order.Carrier = "DHL";

            // Act
            _repository.Update(order);
            _context.SaveChanges();

            // Assert
            var updatedOrder = _context.OrderHeaders.Find(1);
            updatedOrder.TrackingNumber.Should().Be("TRK999999");
            updatedOrder.Carrier.Should().Be("DHL");
        }

        [Fact]
        public void UpdateStatus_ShouldUpdateOrderStatusOnly_WhenPaymentStatusIsNull()
        {
            // Act
            _repository.UpdateStatus(1, "Shipped", null);
            _context.SaveChanges();

            // Assert
            var updatedOrder = _context.OrderHeaders.Find(1);
            updatedOrder.OrderStatus.Should().Be("Shipped");
            updatedOrder.PaymentStatus.Should().Be("Pending"); // Unchanged
        }

        [Fact]
        public void UpdateStatus_ShouldUpdateBothStatuses_WhenPaymentStatusIsProvided()
        {
            // Act
            _repository.UpdateStatus(1, "Shipped", "Paid");
            _context.SaveChanges();

            // Assert
            var updatedOrder = _context.OrderHeaders.Find(1);
            updatedOrder.OrderStatus.Should().Be("Shipped");
            updatedOrder.PaymentStatus.Should().Be("Paid");
        }

        [Fact]
        public void UpdateStatus_ShouldNotThrowException_WhenOrderDoesNotExist()
        {
            // Act & Assert
            var action = () => _repository.UpdateStatus(999, "Shipped", "Paid");
            action.Should().NotThrow();
        }

        [Fact]
        public void UpdateStripePaymentID_ShouldUpdateSessionId_WhenProvided()
        {
            // Act
            _repository.UpdateStripePaymentID(1, "cs_new_session_id", null);
            _context.SaveChanges();

            // Assert
            var updatedOrder = _context.OrderHeaders.Find(1);
            updatedOrder.SessionId.Should().Be("cs_new_session_id");
            updatedOrder.PaymentIntentId.Should().BeNull(); // Unchanged
            updatedOrder.PaymentDate.Should().Be(default(DateTime)); // Unchanged since PaymentIntentId wasn't provided
        }

        [Fact]
        public void UpdateStripePaymentID_ShouldUpdatePaymentIntentIdAndDate_WhenProvided()
        {
            // Arrange
            var beforeUpdate = DateTime.Now;

            // Act
            _repository.UpdateStripePaymentID(1, null, "pi_new_payment_intent");
            _context.SaveChanges();

            // Assert
            var updatedOrder = _context.OrderHeaders.Find(1);
            updatedOrder.PaymentIntentId.Should().Be("pi_new_payment_intent");
            updatedOrder.SessionId.Should().BeNull(); // Unchanged
            updatedOrder.PaymentDate.Should().BeAfter(beforeUpdate);
        }

        [Fact]
        public void UpdateStripePaymentID_ShouldUpdateBoth_WhenBothProvided()
        {
            // Arrange
            var beforeUpdate = DateTime.Now;

            // Act
            _repository.UpdateStripePaymentID(1, "cs_new_session", "pi_new_intent");
            _context.SaveChanges();

            // Assert
            var updatedOrder = _context.OrderHeaders.Find(1);
            updatedOrder.SessionId.Should().Be("cs_new_session");
            updatedOrder.PaymentIntentId.Should().Be("pi_new_intent");
            updatedOrder.PaymentDate.Should().BeAfter(beforeUpdate);
        }

        [Fact]
        public void UpdateStripePaymentID_ShouldNotThrowException_WhenOrderDoesNotExist()
        {
            // Act & Assert
            var action = () => _repository.UpdateStripePaymentID(999, "cs_session", "pi_intent");
            action.Should().NotThrow();
        }

        [Fact]
        public void Get_ShouldReturnOrderHeader_WhenOrderExists()
        {
            // Act
            var order = _repository.Get(o => o.Id == 1);

            // Assert
            order.Should().NotBeNull();
            order.Id.Should().Be(1);
            order.OrderTotal.Should().Be(199.99);
        }

        [Fact]
        public void Get_ShouldReturnNull_WhenOrderDoesNotExist()
        {
            // Act
            var order = _repository.Get(o => o.Id == 999);

            // Assert
            order.Should().BeNull();
        }

        [Fact]
        public void GetAll_ShouldReturnAllOrderHeaders()
        {
            // Act
            var orders = _repository.GetAll();

            // Assert
            orders.Should().HaveCount(2);
            orders.Should().Contain(o => o.Id == 1);
            orders.Should().Contain(o => o.Id == 2);
        }

        [Fact]
        public void GetAll_ShouldReturnFilteredOrders_WhenFilterIsApplied()
        {
            // Act
            var orders = _repository.GetAll(o => o.OrderStatus == "Pending");

            // Assert
            orders.Should().HaveCount(1);
            orders.First().Id.Should().Be(1);
        }

        [Fact]
        public void Add_ShouldAddNewOrderHeader()
        {
            // Arrange
            var newOrder = new OrderHeader
            {
                Id = 3,
                ApplicationUserId = "user3",
                OrderDate = DateTime.Now,
                OrderTotal = 299.99,
                OrderStatus = "New",
                PaymentStatus = "Pending",
                PhoneNumber = "5551234567",
                StreetAddress = "789 Oak St",
                City = "Sometown",
                State = "TX",
                PostalCode = "54321",
                Name = "Bob Johnson"
            };

            // Act
            _repository.Add(newOrder);
            _context.SaveChanges();

            // Assert
            var savedOrder = _context.OrderHeaders.Find(3);
            savedOrder.Should().NotBeNull();
            savedOrder.ApplicationUserId.Should().Be("user3");
            savedOrder.OrderTotal.Should().Be(299.99);
        }

        [Fact]
        public void Remove_ShouldRemoveOrderHeader()
        {
            // Arrange
            var order = _context.OrderHeaders.Find(1);

            // Act
            _repository.Remove(order);
            _context.SaveChanges();

            // Assert
            var deletedOrder = _context.OrderHeaders.Find(1);
            deletedOrder.Should().BeNull();
            _context.OrderHeaders.Count().Should().Be(1);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}