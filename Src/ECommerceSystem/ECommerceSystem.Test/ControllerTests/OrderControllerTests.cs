using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using ECommerceWebApp.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Xunit;

namespace ECommerceSystem.Test.ControllerTests
{
    public class OrderControllerTests
    {
        private readonly Mock<IOrderHeaderService> _mockOrderHeaderService;
        private readonly Mock<IOrderDetailService> _mockOrderDetailService;
        private readonly OrderController _controller;

        public OrderControllerTests()
        {
            _mockOrderHeaderService = new Mock<IOrderHeaderService>();
            _mockOrderDetailService = new Mock<IOrderDetailService>();
            _controller = new OrderController(_mockOrderHeaderService.Object, _mockOrderDetailService.Object);

            // Set up controller context with TempData
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;
        }

        #region Index Tests

        [Fact]
        public void Index_AdminRole_ReturnsViewWithAllOrders()
        {
            // Arrange
            var orderHeaders = GetSampleOrderHeaders();
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser"))
                .Returns(orderHeaders);

            SetupUserRole(_controller, SD.Role_Admin);

            // Act
            var result = _controller.Index(null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(result.Model);
            Assert.Equal(3, model.Count());
        }

        [Fact]
        public void Index_EmployeeRole_ReturnsViewWithAllOrders()
        {
            // Arrange
            var orderHeaders = GetSampleOrderHeaders();
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser"))
                .Returns(orderHeaders);

            SetupUserRole(_controller, SD.Role_Employee);

            // Act
            var result = _controller.Index(null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(result.Model);
            Assert.Equal(3, model.Count());
        }

        [Fact]
        public void Index_CustomerRole_ReturnsViewWithCustomerOrders()
        {
            // Arrange
            var userId = "user123";
            var orderHeaders = GetSampleOrderHeaders().Where(o => o.ApplicationUserId == userId);

            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeadersById(userId, "ApplicationUser"))
                .Returns(orderHeaders);

            SetupUserWithClaims(_controller, userId);

            // Act
            var result = _controller.Index(null) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(result.Model);
            Assert.Single(model);
        }

        [Fact]
        public void Index_InvalidUser_ReturnsUnauthorized()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = _controller.Index(null);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Theory]
        [InlineData("pending", SD.PaymentStatusPending, 1)]
        [InlineData("inprocess", SD.StatusInProcess, 1)]
        [InlineData("completed", SD.StatusShipped, 1)]
        [InlineData("approved", SD.StatusApproved, 0)]
        public void Index_WithStatusFilter_ReturnsFilteredOrders(string status, string filterStatus, int expectedCount)
        {
            // Arrange
            var orderHeaders = GetSampleOrderHeaders();
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser"))
                .Returns(orderHeaders);

            SetupUserRole(_controller, SD.Role_Admin);

            // Act
            var result = _controller.Index(status) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(result.Model);

            if (status == "Pending")
                Assert.Equal(expectedCount, model.Count(o => o.PaymentStatus == filterStatus));
            else
                Assert.Equal(expectedCount, model.Count(o => o.OrderStatus == filterStatus));
        }

        #endregion

        #region Details Tests

        [Fact]
        public void Details_ValidId_ReturnsViewWithOrderVM()
        {
            // Arrange
            int orderId = 1;
            var orderHeader = GetSampleOrderHeaders().First(o => o.Id == orderId);
            var orderDetails = new List<OrderDetail>
            {
                new OrderDetail { Id = 1, OrderHeaderId = orderId, Product = new Product { Title = "Test Product" } }
            };

            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, "ApplicationUser"))
                .Returns(orderHeader);
            _mockOrderDetailService.Setup(s => s.GetAllOrders(orderId, "Product"))
                .Returns(orderDetails);

            // Act
            var result = _controller.Details(orderId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<OrderVM>(result.Model);
            Assert.Equal(orderId, model.orderHeader.Id);
            Assert.Single(model.orderDetails);
        }

        [Fact]
        public void Details_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = _controller.Details(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Details_OrderNotFound_ReturnsNotFound()
        {
            // Arrange
            int orderId = 999;
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, "ApplicationUser"))
                .Returns((OrderHeader)null);

            // Act
            var result = _controller.Details(orderId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Details_Post_UpdatesOrderHeaderSuccessfully()
        {
            // Arrange
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader
                {
                    Id = 1,
                    Name = "Updated Name",
                    PhoneNumber = "5551234567",
                    StreetAddress = "123 Updated St",
                    City = "New City",
                    State = "NS",
                    PostalCode = "12345",
                    Carrier = "Updated Carrier",
                    TrackingNumber = "TRACK123"
                }
            };

            var existingOrderHeader = new OrderHeader { Id = 1 };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderVM.orderHeader.Id,null))
                .Returns(existingOrderHeader);

            // Act
            var result = _controller.Details(orderVM) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
            Assert.Equal(1, result.RouteValues["id"]);

            // Verify the order header was updated
            _mockOrderHeaderService.Verify(s => s.UpdateOrderHeader(It.IsAny<OrderHeader>()), Times.Once);

            // Verify all properties were updated
            Assert.Equal("Updated Name", existingOrderHeader.Name);
            Assert.Equal("5551234567", existingOrderHeader.PhoneNumber);
            Assert.Equal("123 Updated St", existingOrderHeader.StreetAddress);
            Assert.Equal("New City", existingOrderHeader.City);
            Assert.Equal("NS", existingOrderHeader.State);
            Assert.Equal("12345", existingOrderHeader.PostalCode);
            Assert.Equal("Updated Carrier", existingOrderHeader.Carrier);
            Assert.Equal("TRACK123", existingOrderHeader.TrackingNumber);
        }

        [Fact]
        public void Details_Post_WithInvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader { Id = 1 }
            };
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = _controller.Details(orderVM) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderVM, result.Model);
        }

        [Fact]
        public void Details_Post_OrderNotFound_ReturnsNotFound()
        {
            // Arrange
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader { Id = 999 }
            };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderVM.orderHeader.Id,null))
                .Returns((OrderHeader)null);

            // Act
            var result = _controller.Details(orderVM);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region StartProcessing Tests

        [Fact]
        public void StartProcessing_ValidModel_UpdatesStatusAndRedirects()
        {
            // Arrange
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader { Id = 1 }
            };

            // Act
            var result = _controller.StartProcessing(orderVM) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
            Assert.Equal(1, result.RouteValues?["id"]);

            _mockOrderHeaderService.Verify(s => s.UpdateStatus(1, SD.StatusInProcess,null), Times.Once);
        }

        [Fact]
        public void StartProcessing_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader { Id = 1 }
            };
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = _controller.StartProcessing(orderVM) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderVM, result.Model);
        }

        #endregion

        #region ShipOrder Tests

        [Fact]
        public void ShipOrder_ValidModel_UpdatesOrderAndRedirects()
        {
            // Arrange
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader
                {
                    Id = 1,
                    TrackingNumber = "TRACK123",
                    Carrier = "Carrier"
                }
            };

            var existingOrder = new OrderHeader { Id = 1 };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(1, null))
                .Returns(existingOrder);

            // Act
            var result = _controller.ShipOrder(orderVM) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
            Assert.Equal(1, result.RouteValues?["id"]);

            _mockOrderHeaderService.Verify(s => s.UpdateOrderHeader(It.IsAny<OrderHeader>()), Times.Once);

            // Verify properties were updated correctly
            Assert.Equal("TRACK123", existingOrder.TrackingNumber);
            Assert.Equal("Carrier", existingOrder.Carrier);
            Assert.Equal(SD.StatusShipped, existingOrder.OrderStatus);
            Assert.NotNull(existingOrder?.ShippingDate);
        }

        [Fact]
        public void ShipOrder_DelayedPayment_SetsPaymentDueDate()
        {
            // Arrange
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader
                {
                    Id = 1,
                    TrackingNumber = "TRACK123",
                    Carrier = "Carrier"
                }
            };

            var existingOrder = new OrderHeader
            {
                Id = 1,
                PaymentStatus = SD.PaymentStatusDelayedPayment
            };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(1, null))
                .Returns(existingOrder);

            // Act
            var result = _controller.ShipOrder(orderVM) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(existingOrder.PaymentDueDate);
            // Verify due date is approximately 30 days in the future
            var expectedDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            Assert.Equal(expectedDate.Year, existingOrder.PaymentDueDate.Year);
            Assert.Equal(expectedDate.Month, existingOrder.PaymentDueDate.Month);
            Assert.Equal(expectedDate.Day, existingOrder.PaymentDueDate.Day);
        }

        [Fact]
        public void ShipOrder_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader { Id = 1 }
            };
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = _controller.ShipOrder(orderVM);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void ShipOrder_OrderNotFound_ReturnsNotFound()
        {
            // Arrange
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader { Id = 999 }
            };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(999, null))
                .Returns((OrderHeader)null);

            // Act
            var result = _controller.ShipOrder(orderVM);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region CancelOrder Tests

        [Fact]
        public void CancelOrder_RegularOrder_UpdatesStatusAndRedirects()
        {
            // Arrange
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader { Id = 1 }
            };

            var existingOrder = new OrderHeader
            {
                Id = 1,
                PaymentStatus = SD.PaymentStatusPending
            };

            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(1,null))
                .Returns(existingOrder);

            // Act
            var result = _controller.CancelOrder(orderVM) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Details", result.ActionName);
            Assert.Equal(1, result.RouteValues?["id"]);

            _mockOrderHeaderService.Verify(s => s.UpdateStatus(1, SD.StatusCancelled, SD.StatusCancelled), Times.Once);
        }

        [Fact]
        public void CancelOrder_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader { Id = 1 }
            };
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = _controller.CancelOrder(orderVM);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public void CancelOrder_OrderNotFound_ReturnsNotFound()
        {
            // Arrange
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader { Id = 999 }
            };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(999, null))
                .Returns((OrderHeader)null);

            // Act
            var result = _controller.CancelOrder(orderVM);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region PaymentConfirmation Tests

        [Fact]
        public void PaymentConfirmation_ValidId_ReturnsViewWithOrderId()
        {
            // Arrange
            int orderId = 1;
            var orderHeader = new OrderHeader
            {
                Id = orderId,
                PaymentStatus = "Regular"
            };

            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId,null))
                .Returns(orderHeader);

            // Act
            var result = _controller.PaymentConfirmation(orderId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Model);
        }

        [Fact]
        public void PaymentConfirmation_InvalidModelState_ReturnsViewWithOrderId()
        {
            // Arrange
            int orderId = 1;
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = _controller.PaymentConfirmation(orderId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(orderId, result.Model);
        }

        [Fact]
        public void PaymentConfirmation_OrderNotFound_ReturnsNotFound()
        {
            // Arrange
            int orderId = 999;
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, null))
                .Returns((OrderHeader)null);

            // Act
            var result = _controller.PaymentConfirmation(orderId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Helper Methods

        private IEnumerable<OrderHeader> GetSampleOrderHeaders()
        {
            return new List<OrderHeader>
            {
                new OrderHeader
                {
                    Id = 1,
                    ApplicationUserId = "user123",
                    OrderStatus = SD.StatusInProcess,
                    PaymentStatus = SD.PaymentStatusPending
                },
                new OrderHeader
                {
                    Id = 2,
                    ApplicationUserId = "user456",
                    OrderStatus = SD.StatusShipped,
                    PaymentStatus = SD.PaymentStatusApproved
                },
                new OrderHeader
                {
                    Id = 3,
                    ApplicationUserId = "user789",
                    OrderStatus = SD.StatusPending,
                    PaymentStatus = SD.PaymentStatusPending
                }
            };
        }

        private void SetupUserRole(OrderController controller, string role)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.Role, role),
                    new Claim(ClaimTypes.NameIdentifier, "user123")
                }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        private void SetupUserWithClaims(OrderController controller, string userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(
                new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        #endregion
    }

/*    // Dummy implementation of SD class for testing purposes
    public static class SD
    {
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";
        public const string Role_Customer = "Customer";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatusCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";

        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayedPayment = "Delayed";
        public const string PaymentStatusRejected = "Rejected";
    }*/
}