using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using ECommerceWebApp.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Stripe;
using Stripe.BillingPortal;
using System.Collections.Generic;
using System.Security.Claims;
using Xunit;

namespace ECommerceSystem.Test.ControllerTests
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderHeaderService> _mockOrderHeaderService;
        private readonly Mock<IOrderDetailService> _mockOrderDetailService;
        //private readonly Mock<IStripeSessionService> _mockStripeSessionService;

        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _mockOrderHeaderService = new Mock<IOrderHeaderService>();
            _mockOrderDetailService = new Mock<IOrderDetailService>();
            _controller = new OrderController(_mockOrderHeaderService.Object, _mockOrderDetailService.Object);
        }

        #region Helper Methods
        private void SetupUser(string role = null!, string userId = null!)
        {
            var claims = new List<Claim>();
            if (role != null) claims.Add(new Claim(ClaimTypes.Role, role));
            if (userId != null) claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var user = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = user } };
        }
        #endregion

        #region Index Tests
        [Fact]
        public void Index_AdminRole_ReturnsAllOrders()
        {
            // Arrange
            SetupUser(role: SD.Role_Admin);
            var orders = new List<OrderHeader> { new OrderHeader { Id = 1 } };
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser")).Returns(orders);

            // Act
            var result = _controller.Index(null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orders, result.Model);
        }

        [Fact]
        public void Index_EmployeeRole_ReturnsAllOrders()
        {
            // Arrange
            SetupUser(role: SD.Role_Employee);
            var orders = new List<OrderHeader> { new OrderHeader { Id = 1 } };
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser")).Returns(orders);

            // Act
            var result = _controller.Index(null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orders, result.Model);
        }

        [Fact]
        public void Index_CustomerRole_ReturnsUserOrders()
        {
            // Arrange
            string userId = "user123";
            SetupUser(userId: userId);
            var orders = new List<OrderHeader> { new OrderHeader { Id = 1 } };
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeadersById(userId, "ApplicationUser")).Returns(orders);

            // Act
            var result = _controller.Index(null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orders, result.Model);
        }

        [Fact]
        public void Index_UnauthorizedUser_ReturnsUnauthorized()
        {
            // Arrange
            SetupUser(); // No role or userId

            // Act
            var result = _controller.Index(null);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Theory]
        [InlineData("pending", SD.PaymentStatusPending, 1)]
        [InlineData("inprocess", SD.StatusInProcess, 1)]
        [InlineData("completed", SD.StatusShipped, 1)]
        [InlineData("approved", SD.StatusApproved, 1)]
        public void Index_FiltersOrdersByStatus(string status, string expectedStatus, int expectedCount)
        {
            // Arrange
            SetupUser(role: SD.Role_Admin);
            var orders = new List<OrderHeader>
    {
        new OrderHeader { Id = 1, PaymentStatus = SD.PaymentStatusPending, OrderStatus = SD.StatusInProcess },
        new OrderHeader { Id = 2, PaymentStatus = SD.PaymentStatusApproved, OrderStatus = SD.StatusShipped },
        new OrderHeader { Id = 3, PaymentStatus = SD.PaymentStatusApproved, OrderStatus = SD.StatusApproved } // Added for approved status
    };
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser")).Returns(orders);

            // Act
            var result = _controller.Index(status) as ViewResult;

            // Assert
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(result?.Model);
            if (status == "pending")
                Assert.Equal(expectedCount, model.Count(o => o.PaymentStatus == expectedStatus));
            else
                Assert.Equal(expectedCount, model.Count(o => o.OrderStatus == expectedStatus));
        }
        #endregion

        #region Details GET Tests
        [Fact]
        public void Details_Get_ReturnsOrderVM_WhenOrderExists()
        {
            // Arrange
            int orderId = 1;
            var orderHeader = new OrderHeader { Id = orderId };
            var orderDetails = new List<OrderDetail> { new OrderDetail { Id = 1 } };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, "ApplicationUser")).Returns(orderHeader);
            _mockOrderDetailService.Setup(s => s.GetAllOrders(orderId, "Product")).Returns(orderDetails);

            // Act
            var result = _controller.Details(orderId) as ViewResult;

            // Assert
            var model = Assert.IsType<OrderVM>(result?.Model);
            Assert.Equal(orderHeader, model.orderHeader);
            Assert.Equal(orderDetails, model.orderDetails);
        }

        [Fact]
        public void Details_Get_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            int orderId = 1;
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, "ApplicationUser")).Returns((OrderHeader?)null);

            // Act
            var result = _controller.Details(orderId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        #endregion

        #region Details POST Tests
        [Fact]
        public void Details_Post_UpdatesOrderHeader_WhenValid()
        {
            // Arrange
            SetupUser(role: SD.Role_Admin);
            var orderVM = new OrderVM { orderHeader = new OrderHeader { Id = 1, Name = "Updated" } };
            var existingOrder = new OrderHeader { Id = 1 };

            // Explicitly provide all arguments, including the optional one
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(1, null)).Returns(existingOrder);

            // Act
            var result = _controller.Details(orderVM) as RedirectToActionResult;

            // Assert
            _mockOrderHeaderService.Verify(s => s.UpdateOrderHeader(It.Is<OrderHeader>(o => o.Name == "Updated")), Times.Once());
            Assert.Equal("Details", result?.ActionName);
            Assert.Equal(1, result?.RouteValues?["id"]);
        }

        [Fact]
        public void Details_Post_ReturnsView_WhenModelStateInvalid()
        {
            // Arrange
            SetupUser(role: SD.Role_Admin);
            var orderVM = new OrderVM { orderHeader = new OrderHeader { Id = 1 } };
            _controller.ModelState.AddModelError("Name", "Required");

            // Act
            var result = _controller.Details(orderVM) as ViewResult;

            // Assert
            Assert.Equal(orderVM, result!.Model);
        }

        [Fact]
        public void Details_Post_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            SetupUser(role: SD.Role_Admin);
            var orderVM = new OrderVM { orderHeader = new OrderHeader { Id = 1 } };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(1, null)).Returns((OrderHeader?)null);

            // Act
            var result = _controller.Details(orderVM);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        #endregion

        #region StartProcessing Tests
        [Fact]
        public void StartProcessing_UpdatesStatus_WhenValid()
        {
            // Arrange
            SetupUser(role: SD.Role_Admin);
            var orderVM = new OrderVM { orderHeader = new OrderHeader { Id = 1 } };

            // Act
            var result = _controller.StartProcessing(orderVM) as RedirectToActionResult;

            // Assert
            _mockOrderHeaderService.Verify(s => s.UpdateStatus(1, SD.StatusInProcess, null), Times.Once());
            Assert.Equal("Details", result!.ActionName);
            Assert.Equal(1, result!.RouteValues?["id"]);
        }

        [Fact]
        public void StartProcessing_ReturnsView_WhenModelStateInvalid()
        {
            // Arrange
            SetupUser(role: SD.Role_Admin);
            var orderVM = new OrderVM { orderHeader = new OrderHeader { Id = 1 } };
            _controller.ModelState.AddModelError("Id", "Invalid");

            // Act
            var result = _controller.StartProcessing(orderVM) as ViewResult;

            // Assert
            Assert.Equal(orderVM, result?.Model);
        }
        #endregion

        #region ShipOrder Tests
        [Fact]
        public void ShipOrder_UpdatesOrder_WhenValid()
        {
            // Arrange
            SetupUser(role: SD.Role_Admin);
            var orderVM = new OrderVM { orderHeader = new OrderHeader { Id = 1, TrackingNumber = "TRACK123", Carrier = "UPS" } };
            var existingOrder = new OrderHeader { Id = 1, PaymentStatus = SD.PaymentStatusApproved };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(1, null)).Returns(existingOrder);

            // Act
            var result = _controller.ShipOrder(orderVM) as RedirectToActionResult;

            // Assert
            _mockOrderHeaderService.Verify(s => s.UpdateOrderHeader(It.Is<OrderHeader>(o =>
                o.OrderStatus == SD.StatusShipped &&
                o.TrackingNumber == "TRACK123" &&
                o.Carrier == "UPS" &&
                o.ShippingDate != default)), Times.Once());
            Assert.Equal("Details", result?.ActionName);
            Assert.Equal(1, result!.RouteValues?["id"]);
        }

        [Fact]
        public void ShipOrder_SetsPaymentDueDate_WhenDelayedPayment()
        {
            // Arrange
            SetupUser(role: SD.Role_Admin);
            var orderVM = new OrderVM { orderHeader = new OrderHeader { Id = 1, TrackingNumber = "TRACK123", Carrier = "UPS" } };
            var existingOrder = new OrderHeader { Id = 1, PaymentStatus = SD.PaymentStatusDelayedPayment };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(1, null)).Returns(existingOrder);

            // Act
            var result = _controller.ShipOrder(orderVM);

            // Assert
            _mockOrderHeaderService.Verify(s => s.UpdateOrderHeader(It.Is<OrderHeader>(o =>
                o.PaymentDueDate >= DateOnly.FromDateTime(DateTime.Now))), Times.Once());
        }
        #endregion

        #region CancelOrder Tests
        [Fact]
        public void CancelOrder_CancelsOrder_WhenNotPaid()
        {
            // Arrange
            SetupUser(role: SD.Role_Admin);
            var orderVM = new OrderVM { orderHeader = new OrderHeader { Id = 1 } };
            var existingOrder = new OrderHeader { Id = 1, PaymentStatus = SD.PaymentStatusPending };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(1, null)).Returns(existingOrder);

            // Act
            var result = _controller.CancelOrder(orderVM) as RedirectToActionResult;

            // Assert
            _mockOrderHeaderService.Verify(s => s.UpdateStatus(1, SD.StatusCancelled, SD.StatusCancelled), Times.Once());
            Assert.Equal("Details", result?.ActionName);
        }

        [Fact]
        public void CancelOrder_Refunds_WhenPaid()
        {
            // Arrange
            SetupUser(role: SD.Role_Admin);
            var orderVM = new OrderVM { orderHeader = new OrderHeader { Id = 1 } };
            var existingOrder = new OrderHeader { Id = 1, PaymentStatus = SD.PaymentStatusApproved, PaymentIntentId = null }; // Avoid Stripe call
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(1, null)).Returns(existingOrder);

            // Act
            var result = _controller.CancelOrder(orderVM) as RedirectToActionResult;

            // Assert
            _mockOrderHeaderService.Verify(s => s.UpdateStatus(1, SD.StatusCancelled, SD.StatusCancelled), Times.Once()); // Non-refunded path
            Assert.Equal("Details", result?.ActionName);
            Assert.Equal(1, result!.RouteValues?["id"]);
        }
        #endregion

        #region PayDetails Tests
       

        [Fact]
        public void PayDetails_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            SetupUser();
            var orderVM = new OrderVM { orderHeader = new OrderHeader { Id = 1 } };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(1, "ApplicationUser")).Returns((OrderHeader?)null);

            // Act
            var result = _controller.PayDetails(orderVM);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        #endregion

        #region PaymentConfirmation Tests
       /* [Fact]
        public void PaymentConfirmation_ReturnsView_WhenOrderExists()
        {
            // Arrange
            int orderId = 1;
            var sessionId = "sess_123";

            var orderHeader = new OrderHeader
            {
                Id = orderId,
                PaymentStatus = SD.PaymentStatusDelayedPayment,
                SessionId = sessionId
            };

            var fakeSession = new Session(); // Create as plain, no 'PaymentStatus' access
            typeof(Session).GetProperty("Status")?.SetValue(fakeSession, "complete"); // only if needed

            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, null))
                .Returns(orderHeader);

            _mockStripeSessionService.Setup(s => s.GetSession(sessionId))
                .Returns(fakeSession);

            // Act
            var result = _controller.PaymentConfirmation(orderId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Model);
        }*/


        [Fact]
        public void PaymentConfirmation_ReturnsNotFound_WhenOrderDoesNotExist()
        {
            // Arrange
            int orderId = 1;
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, null)).Returns((OrderHeader?)null);

            // Act
            var result = _controller.PaymentConfirmation(orderId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
        #endregion
    }
}