using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace ECommerceSystem.Test.ServiceTests
{
    // First, let's define an interface for the Stripe SessionService to make it testable
    public interface IStripeSessionService
    {
        Stripe.Checkout.Session Get(string sessionId);
    }

    // Wrapper for the real Stripe SessionService
    public class StripeSessionServiceWrapper : IStripeSessionService
    {
        private readonly Stripe.Checkout.SessionService _sessionService;

        public StripeSessionServiceWrapper()
        {
            _sessionService = new Stripe.Checkout.SessionService();
        }

        public Stripe.Checkout.Session Get(string sessionId)
        {
            return _sessionService.Get(sessionId);
        }
    }

    // Update the OrderHeaderService to use the interface (this is the modified version)
    public class TestableOrderHeaderService : OrderHeaderService
    {
        private readonly IStripeSessionService _sessionService;

        public class OrderHeaderService
        {
            protected readonly IUnitOfWork _unitOfWork;

            public OrderHeaderService(
                IOrderHeaderRepository orderHeaderRepository,
                IUnitOfWork unitOfWork,
                IHttpContextAccessor httpContextAccessor)
            {
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
                // Other initialization
            }
        }

        // Override the OrderConfirmation method to use the interface
        // Override the OrderConfirmation method to use the interface
        public new OrderHeader? OrderConfirmation(int id)
        {
            var orderHeader = GetOrderHeaderById(id, "ApplicationUser");
            if (orderHeader != null && orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                if (orderHeader.SessionId != null)
                {
                    var session = _sessionService.Get(orderHeader.SessionId);
                    if (string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                        UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                        _unitOfWork.Commit();
                    }
                }
                _httpContextAccessor.HttpContext?.Session.Clear();
            }
            return orderHeader;
        }

        // Expose UnitOfWork for testing
        protected IUnitOfWork _unitOfWork;

        // Expose HttpContextAccessor for testing
        protected IHttpContextAccessor _httpContextAccessor;
    }

    public class OrderHeaderServiceTests
    {
        private readonly Mock<IOrderHeaderRepository> _mockOrderHeaderRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IStripeSessionService> _mockStripeSessionService;
        private readonly TestableOrderHeaderService _orderHeaderService;
        private readonly Mock<ISession> _mockSession;
        private readonly Mock<HttpContext> _mockHttpContext;

        public OrderHeaderServiceTests()
        {
            _mockOrderHeaderRepository = new Mock<IOrderHeaderRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockStripeSessionService = new Mock<IStripeSessionService>();
            _mockSession = new Mock<ISession>();
            _mockHttpContext = new Mock<HttpContext>();

            _mockHttpContext.Setup(x => x.Session).Returns(_mockSession.Object);
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(_mockHttpContext.Object);

            _orderHeaderService = new TestableOrderHeaderService(
                _mockOrderHeaderRepository.Object,
                _mockUnitOfWork.Object,
                _mockHttpContextAccessor.Object,
                _mockStripeSessionService.Object
            );
        }

        [Fact]
        public void AddOrderHeader_WithValidOrderHeader_ShouldAddOrderHeader()
        {
            // Arrange
            var orderHeader = new OrderHeader { Id = 1, OrderTotal = 100 };

            // Act
            _orderHeaderService.AddOrderHeader(orderHeader);

            // Assert
            _mockOrderHeaderRepository.Verify(r => r.Add(orderHeader), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void DeleteOrderHeader_WithValidId_ShouldDeleteOrderHeader()
        {
            // Arrange
            var orderHeader = new OrderHeader { Id = 1, OrderTotal = 100 };
            _mockOrderHeaderRepository.Setup(r => r.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>(), false))
                .Returns(orderHeader);

            // Act
            _orderHeaderService.DeleteOrderHeader(1);

            // Assert
            _mockOrderHeaderRepository.Verify(r => r.Remove(orderHeader), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void DeleteOrderHeader_WithNullId_ShouldNotDeleteOrderHeader()
        {
            // Act
            _orderHeaderService.DeleteOrderHeader(null);

            // Assert
            _mockOrderHeaderRepository.Verify(r => r.Remove(It.IsAny<OrderHeader>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteOrderHeader_WithNonExistentId_ShouldNotDeleteOrderHeader()
        {
            // Arrange
            _mockOrderHeaderRepository.Setup(r => r.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>(), false))
                .Returns((OrderHeader?)null);

            // Act
            _orderHeaderService.DeleteOrderHeader(999);

            // Assert
            _mockOrderHeaderRepository.Verify(r => r.Remove(It.IsAny<OrderHeader>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void GetAllOrderHeaders_WithoutIncludeProperties_ShouldReturnAllOrderHeaders()
        {
            // Arrange
            var expectedOrderHeaders = new List<OrderHeader>
            {
                new OrderHeader { Id = 1, OrderTotal = 100 },
                new OrderHeader { Id = 2, OrderTotal = 200 }
            };

            _mockOrderHeaderRepository.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>()))
                .Returns(expectedOrderHeaders);

            // Act
            var result = _orderHeaderService.GetAllOrderHeaders();

            // Assert
            Assert.Equal(expectedOrderHeaders, result);
            _mockOrderHeaderRepository.Verify(r => r.GetAll(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.Is<string>(s => s == null)), Times.Once);
        }

        [Fact]
        public void GetAllOrderHeaders_WithIncludeProperties_ShouldReturnAllOrderHeadersWithIncludes()
        {
            // Arrange
            var expectedOrderHeaders = new List<OrderHeader>
            {
                new OrderHeader { Id = 1, OrderTotal = 100 },
                new OrderHeader { Id = 2, OrderTotal = 200 }
            };

            _mockOrderHeaderRepository.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>()))
                .Returns(expectedOrderHeaders);

            // Act
            var result = _orderHeaderService.GetAllOrderHeaders("ApplicationUser");

            // Assert
            Assert.Equal(expectedOrderHeaders, result);
            _mockOrderHeaderRepository.Verify(r => r.GetAll(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.Is<string>(s => s == "ApplicationUser")), Times.Once);
        }

        [Fact]
        public void GetOrderHeaderById_WithValidIdWithoutIncludeProperty_ShouldReturnOrderHeader()
        {
            // Arrange
            var expectedOrderHeader = new OrderHeader { Id = 1, OrderTotal = 100 };
            _mockOrderHeaderRepository.Setup(r => r.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>(), false))
                .Returns(expectedOrderHeader);

            // Act
            var result = _orderHeaderService.GetOrderHeaderById(1);

            // Assert
            Assert.Equal(expectedOrderHeader, result);
            _mockOrderHeaderRepository.Verify(r => r.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.Is<string>(s => s == null), false), Times.Once);
        }

        [Fact]
        public void GetOrderHeaderById_WithValidIdWithIncludeProperty_ShouldReturnOrderHeaderWithIncludes()
        {
            // Arrange
            var expectedOrderHeader = new OrderHeader { Id = 1, OrderTotal = 100 };
            _mockOrderHeaderRepository.Setup(r => r.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.IsAny<string>(), false))
                .Returns(expectedOrderHeader);

            // Act
            var result = _orderHeaderService.GetOrderHeaderById(1, "ApplicationUser");

            // Assert
            Assert.Equal(expectedOrderHeader, result);
            _mockOrderHeaderRepository.Verify(r => r.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.Is<string>(s => s == "ApplicationUser"), false), Times.Once);
        }

        [Fact]
        public void UpdateOrderHeader_WithValidOrderHeader_ShouldUpdateOrderHeader()
        {
            // Arrange
            var orderHeader = new OrderHeader { Id = 1, OrderTotal = 100 };

            // Act
            _orderHeaderService.UpdateOrderHeader(orderHeader);

            // Assert
            _mockOrderHeaderRepository.Verify(r => r.Update(orderHeader), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateStatus_WithValidParameters_ShouldUpdateStatus()
        {
            // Arrange
            int orderId = 1;
            string orderStatus = "Shipped";
            string paymentStatus = "Paid";

            // Act
            _orderHeaderService.UpdateStatus(orderId, orderStatus, paymentStatus);

            // Assert
            _mockOrderHeaderRepository.Verify(r => r.UpdateStatus(orderId, orderStatus, paymentStatus), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateStatus_WithNullPaymentStatus_ShouldUpdateStatusWithNullPayment()
        {
            // Arrange
            int orderId = 1;
            string orderStatus = "Shipped";

            // Act
            _orderHeaderService.UpdateStatus(orderId, orderStatus);

            // Assert
            _mockOrderHeaderRepository.Verify(r => r.UpdateStatus(orderId, orderStatus, null), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateStripePaymentID_WithValidParameters_ShouldUpdateStripePaymentID()
        {
            // Arrange
            int orderId = 1;
            string sessionId = "session_123";
            string paymentIntentId = "pi_123";

            // Act
            _orderHeaderService.UpdateStripePaymentID(orderId, sessionId, paymentIntentId);

            // Assert
            _mockOrderHeaderRepository.Verify(r => r.UpdateStripePaymentID(orderId, sessionId, paymentIntentId), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

       /* [Fact]
        public void OrderConfirmation_WithValidIdAndPaidStatus_ShouldUpdateOrderAndClearSession()
        {
            // Arrange
            var orderHeader = new OrderHeader
            {
                Id = 1,
                OrderTotal = 100,
                SessionId = "session_123",
                PaymentStatus = "Processing"
            };

            _mockOrderHeaderRepository
                .Setup(r => r.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.Is<string>(s => s == "ApplicationUser"), false))
                .Returns(orderHeader);

            // Mock the Stripe Session
            var mockSession = new Stripe.Checkout.Session
            {
                Id = "session_123",
                PaymentIntentId = "pi_123",
                PaymentStatus = "paid"
            };

            _mockStripeSessionService
                .Setup(s => s.Get(orderHeader.SessionId))
                .Returns(mockSession);

            // Setup Session.Clear() to avoid NullReferenceException
            _mockSession.Setup(s => s.Clear());

            // Act
            var result = _orderHeaderService.OrderConfirmation(1);

            // Assert
            Assert.Equal(orderHeader, result);
            _mockOrderHeaderRepository.Verify(r => r.UpdateStripePaymentID(1, mockSession.Id, mockSession.PaymentIntentId), Times.Once);
            _mockOrderHeaderRepository.Verify(r => r.UpdateStatus(1, SD.StatusApproved, SD.PaymentStatusApproved), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.AtLeastOnce);
            _mockSession.Verify(s => s.Clear(), Times.Once);
        }


        [Fact]
        public void OrderConfirmation_WithValidIdAndDelayedPayment_ShouldNotUpdateOrderButClearSession()
        {
            // Arrange
            var orderHeader = new OrderHeader
            {
                Id = 1,
                OrderTotal = 100,
                SessionId = "session_123",
                PaymentStatus = SD.PaymentStatusDelayedPayment
            };

            _mockOrderHeaderRepository.Setup(r => r.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.Is<string>(s => s == "ApplicationUser"), false))
                .Returns(orderHeader);

            // Act
            var result = _orderHeaderService.OrderConfirmation(1);

            // Assert
            Assert.Equal(orderHeader, result);
            _mockOrderHeaderRepository.Verify(r => r.UpdateStripePaymentID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockOrderHeaderRepository.Verify(r => r.UpdateStatus(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
            _mockSession.Verify(s => s.Clear(), Times.Once);
        }*/

        [Fact]
        public void OrderConfirmation_WithNullOrderHeader_ShouldReturnNull()
        {
            // Arrange
            _mockOrderHeaderRepository.Setup(r => r.Get(It.IsAny<Expression<Func<OrderHeader, bool>>>(), It.Is<string>(s => s == "ApplicationUser"), false))
                .Returns((OrderHeader?)null);

            // Act
            var result = _orderHeaderService.OrderConfirmation(1);

            // Assert
            Assert.Null(result);
            _mockOrderHeaderRepository.Verify(r => r.UpdateStripePaymentID(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockOrderHeaderRepository.Verify(r => r.UpdateStatus(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
            _mockSession.Verify(s => s.Clear(), Times.Never);
        }

        [Fact]
        public void GetAllOrderHeadersById_WithValidUserId_ShouldReturnUserOrderHeaders()
        {
            // Arrange
            var userId = "user123";
            var expectedOrderHeaders = new List<OrderHeader>
            {
                new OrderHeader { Id = 1, OrderTotal = 100, ApplicationUserId = userId },
                new OrderHeader { Id = 2, OrderTotal = 200, ApplicationUserId = userId }
            };

            _mockOrderHeaderRepository.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>()))
                .Returns(expectedOrderHeaders);

            // Act
            var result = _orderHeaderService.GetAllOrderHeadersById(userId);

            // Assert
            Assert.Equal(expectedOrderHeaders, result);
            _mockOrderHeaderRepository.Verify(r => r.GetAll(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.Is<string>(s => s == null)), Times.Once);
        }

        [Fact]
        public void GetAllOrderHeadersById_WithValidUserIdAndIncludes_ShouldReturnUserOrderHeadersWithIncludes()
        {
            // Arrange
            var userId = "user123";
            var expectedOrderHeaders = new List<OrderHeader>
            {
                new OrderHeader { Id = 1, OrderTotal = 100, ApplicationUserId = userId },
                new OrderHeader { Id = 2, OrderTotal = 200, ApplicationUserId = userId }
            };

            _mockOrderHeaderRepository.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.IsAny<string>()))
                .Returns(expectedOrderHeaders);

            // Act
            var result = _orderHeaderService.GetAllOrderHeadersById(userId, "ApplicationUser");

            // Assert
            Assert.Equal(expectedOrderHeaders, result);
            _mockOrderHeaderRepository.Verify(r => r.GetAll(
                It.IsAny<Expression<Func<OrderHeader, bool>>>(),
                It.Is<string>(s => s == "ApplicationUser")), Times.Once);
        }
    }
}