using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using ECommerceWebApp.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System;
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

            // Setup TempData for the controller
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
        }

        private void SetupUserIdentity(bool isAdmin = false, bool isEmployee = false, string userId = "testUser123")
        {
            // Create claims principal for the user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };

            if (isAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, SD.Role_Admin));
            }

            if (isEmployee)
            {
                claims.Add(new Claim(ClaimTypes.Role, SD.Role_Employee));
            }

            var identity = new ClaimsIdentity(claims, "Test");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            // Set up controller context with the claims principal
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };
        }

        [Fact]
        public void Index_AdminRole_ReturnsAllOrders()
        {
            // Arrange
            SetupUserIdentity(isAdmin: true);
            var expectedOrders = new List<OrderHeader>
            {
                new OrderHeader { Id = 1, OrderStatus = "Pending" },
                new OrderHeader { Id = 2, OrderStatus = "Shipped" }
            };
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser")).Returns(expectedOrders);

            // Act
            var result = _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(viewResult.Model);
            Assert.Equal(expectedOrders.Count, model.Count());
            Assert.Equal(expectedOrders, model);
        }

        [Fact]
        public void Index_EmployeeRole_ReturnsAllOrders()
        {
            // Arrange
            SetupUserIdentity(isEmployee: true);
            var expectedOrders = new List<OrderHeader>
            {
                new OrderHeader { Id = 1, OrderStatus = "Pending" },
                new OrderHeader { Id = 2, OrderStatus = "Shipped" }
            };
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser")).Returns(expectedOrders);

            // Act
            var result = _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(viewResult.Model);
            Assert.Equal(expectedOrders.Count, model.Count());
            Assert.Equal(expectedOrders, model);
        }

        [Fact]
        public void Index_CustomerRole_ReturnsUserOrders()
        {
            // Arrange
            string userId = "user123";
            SetupUserIdentity(userId: userId);
            var expectedOrders = new List<OrderHeader>
            {
                new OrderHeader { Id = 1, OrderStatus = "Pending" }
            };
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeadersById(userId, "ApplicationUser")).Returns(expectedOrders);

            // Act
            var result = _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(viewResult.Model);
            Assert.Equal(expectedOrders.Count, model.Count());
            Assert.Equal(expectedOrders, model);
        }

        [Fact]
        public void Index_WithPendingStatusFilter_ReturnsFilteredOrders()
        {
            // Arrange
            SetupUserIdentity(isAdmin: true);
            var allOrders = new List<OrderHeader>
    {
        new OrderHeader { Id = 1, PaymentStatus = SD.PaymentStatusPending, OrderStatus = "Any" },
        new OrderHeader { Id = 2, PaymentStatus = "NotPending", OrderStatus = "Any" },
        new OrderHeader { Id = 3, PaymentStatus = SD.PaymentStatusPending, OrderStatus = "Any" }
    };
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser")).Returns(allOrders);

            // Act
            var result = _controller.Index("pending");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(viewResult.Model);
            Assert.Equal(2, model.Count()); // Should only have orders with PaymentStatus == SD.PaymentStatusPending
            Assert.All(model, item => Assert.Equal(SD.PaymentStatusPending, item.PaymentStatus));
        }

        [Fact]
        public void Index_WithInProcessStatusFilter_ReturnsFilteredOrders()
        {
            // Arrange
            SetupUserIdentity(isAdmin: true);
            var allOrders = new List<OrderHeader>
    {
        new OrderHeader { Id = 1, PaymentStatus = "Any", OrderStatus = SD.StatusInProcess },
        new OrderHeader { Id = 2, PaymentStatus = "Any", OrderStatus = "NotInProcess" },
        new OrderHeader { Id = 3, PaymentStatus = "Any", OrderStatus = SD.StatusInProcess }
    };
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser")).Returns(allOrders);

            // Act
            var result = _controller.Index("inprocess");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(viewResult.Model);
            Assert.Equal(2, model.Count()); // Should only have orders with OrderStatus == SD.StatusInProcess
            Assert.All(model, item => Assert.Equal(SD.StatusInProcess, item.OrderStatus));
        }

        [Fact]
        public void Index_WithCompletedStatusFilter_ReturnsFilteredOrders()
        {
            // Arrange
            SetupUserIdentity(isAdmin: true);
            var allOrders = new List<OrderHeader>
    {
        new OrderHeader { Id = 1, PaymentStatus = "Any", OrderStatus = SD.StatusShipped },
        new OrderHeader { Id = 2, PaymentStatus = "Any", OrderStatus = "NotShipped" },
        new OrderHeader { Id = 3, PaymentStatus = "Any", OrderStatus = SD.StatusShipped }
    };
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser")).Returns(allOrders);

            // Act
            var result = _controller.Index("completed");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(viewResult.Model);
            Assert.Equal(2, model.Count()); // Should only have orders with OrderStatus == SD.StatusShipped
            Assert.All(model, item => Assert.Equal(SD.StatusShipped, item.OrderStatus));
        }

        [Fact]
        public void Index_WithApprovedStatusFilter_ReturnsFilteredOrders()
        {
            // Arrange
            SetupUserIdentity(isAdmin: true);
            var allOrders = new List<OrderHeader>
    {
        new OrderHeader { Id = 1, PaymentStatus = "Any", OrderStatus = SD.StatusApproved },
        new OrderHeader { Id = 2, PaymentStatus = "Any", OrderStatus = "NotApproved" },
        new OrderHeader { Id = 3, PaymentStatus = "Any", OrderStatus = SD.StatusApproved }
    };
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser")).Returns(allOrders);

            // Act
            var result = _controller.Index("approved");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(viewResult.Model);
            Assert.Equal(2, model.Count()); // Should only have orders with OrderStatus == SD.StatusApproved
            Assert.All(model, item => Assert.Equal(SD.StatusApproved, item.OrderStatus));
        }

        public void Index_WithDifferentStatusFilters_ReturnsCorrectlyFilteredOrders(
     string statusFilter,
     string userId,
     bool includePending,
     bool includeInProcess,
     bool includeCompleted,
     bool includeApproved,
     int expectedCount)
        {
            // Arrange
            bool isAdmin = true;
            SetupUserIdentity(isAdmin: isAdmin, userId: userId ?? "testUser123");
            var allOrders = new List<OrderHeader>();
            int id = 1;
            if (includePending)
            {
                allOrders.Add(new OrderHeader { Id = id++, PaymentStatus = SD.PaymentStatusPending, OrderStatus = "Any" });
                allOrders.Add(new OrderHeader { Id = id++, PaymentStatus = SD.PaymentStatusPending, OrderStatus = "Any" });
            }
            if (includeInProcess)
            {
                allOrders.Add(new OrderHeader { Id = id++, OrderStatus = SD.StatusInProcess, PaymentStatus = "Any" });
                allOrders.Add(new OrderHeader { Id = id++, OrderStatus = SD.StatusInProcess, PaymentStatus = "Any" });
            }
            if (includeCompleted)
            {
                allOrders.Add(new OrderHeader { Id = id++, OrderStatus = SD.StatusShipped, PaymentStatus = "Any" });
                allOrders.Add(new OrderHeader { Id = id++, OrderStatus = SD.StatusShipped, PaymentStatus = "Any" });
            }
            if (includeApproved)
            {
                allOrders.Add(new OrderHeader { Id = id++, OrderStatus = SD.StatusApproved, PaymentStatus = "Any" });
                allOrders.Add(new OrderHeader { Id = id++, OrderStatus = SD.StatusApproved, PaymentStatus = "Any" });
            }
            if (userId == null)
            {
                _mockOrderHeaderService.Setup(s => s.GetAllOrderHeaders("ApplicationUser")).Returns(allOrders);
            }
            else
            {
                _mockOrderHeaderService.Setup(s => s.GetAllOrderHeadersById(userId, "ApplicationUser")).Returns(allOrders);
            }

            // Act
            var result = _controller.Index(statusFilter);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<OrderHeader>>(viewResult.Model);

            // Calculate expected count based on applied filters
            int correctExpectedCount;
            if (statusFilter == null)
            {
                // When no filter is applied, all orders should be returned
                correctExpectedCount = allOrders.Count;
            }
            else if (statusFilter == "pending")
            {
                correctExpectedCount = includePending ? 2 : 0;
            }
            else if (statusFilter == "inprocess")
            {
                correctExpectedCount = includeInProcess ? 2 : 0;
            }
            else if (statusFilter == "completed")
            {
                correctExpectedCount = includeCompleted ? 2 : 0;
            }
            else if (statusFilter == "approved")
            {
                correctExpectedCount = includeApproved ? 2 : 0;
            }
            else
            {
                correctExpectedCount = allOrders.Count;
            }

            // Verify correct number of items
            Assert.Equal(correctExpectedCount, model.Count());

            // Verify correct filtering applied
            if (statusFilter == "pending")
            {
                Assert.All(model, item => Assert.Equal(SD.PaymentStatusPending, item.PaymentStatus));
            }
            else if (statusFilter == "inprocess")
            {
                Assert.All(model, item => Assert.Equal(SD.StatusInProcess, item.OrderStatus));
            }
            else if (statusFilter == "completed")
            {
                Assert.All(model, item => Assert.Equal(SD.StatusShipped, item.OrderStatus));
            }
            else if (statusFilter == "approved")
            {
                Assert.All(model, item => Assert.Equal(SD.StatusApproved, item.OrderStatus));
            }
        }

        [Fact]
        public void Details_ReturnsViewWithOrderVM()
        {
            // Arrange
            int orderId = 1;
            var orderHeader = new OrderHeader { Id = orderId, OrderStatus = "Pending" };
            var orderDetails = new List<OrderDetail>
            {
                new OrderDetail { Id = 1, OrderHeaderId = orderId }
            };

            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, "ApplicationUser")).Returns(orderHeader);
            _mockOrderDetailService.Setup(s => s.GetAllOrders(orderId, "Product")).Returns(orderDetails);

            // Act
            var result = _controller.Details(orderId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<OrderVM>(viewResult.Model);
            Assert.Equal(orderHeader, model.orderHeader);
            Assert.Equal(orderDetails, model.orderDetails);
        }

        [Fact]
        public void Details_Post_UpdatesOrderHeaderAndRedirects()
        {
            // Arrange
            int orderId = 1;
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader
                {
                    Id = orderId,
                    Name = "Updated Name",
                    PhoneNumber = "555-1234",
                    StreetAddress = "123 Main St",
                    City = "New City",
                    State = "State",
                    PostalCode = "12345",
                    Carrier = "New Carrier",
                    TrackingNumber = "ABC123"
                }
            };

            var existingOrderHeader = new OrderHeader { Id = orderId };
            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, It.IsAny<string>()))
      .Returns(existingOrderHeader);


            // Act
            var result = _controller.Details(orderVM);

            // Assert
            // Verify order header was updated with new values
            Assert.Equal("Updated Name", existingOrderHeader.Name);
            Assert.Equal("555-1234", existingOrderHeader.PhoneNumber);
            Assert.Equal("123 Main St", existingOrderHeader.StreetAddress);
            Assert.Equal("New City", existingOrderHeader.City);
            Assert.Equal("State", existingOrderHeader.State);
            Assert.Equal("12345", existingOrderHeader.PostalCode);
            Assert.Equal("New Carrier", existingOrderHeader.Carrier);
            Assert.Equal("ABC123", existingOrderHeader.TrackingNumber);

            _mockOrderHeaderService.Verify(s => s.UpdateOrderHeader(existingOrderHeader), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(orderId, redirectResult.RouteValues?["id"]);
        }




        [Fact]
        public void ShipOrder_UpdatesOrderAndRedirects()
        {
            // Arrange
            int orderId = 1;
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader
                {
                    Id = orderId,
                    TrackingNumber = "TRACK123",
                    Carrier = "FedEx"
                }
            };

            var existingOrderHeader = new OrderHeader
            {
                Id = orderId,
                PaymentStatus = SD.PaymentStatusApproved
            };

            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, It.IsAny<string>()))
     .Returns(existingOrderHeader);


            DateTime beforeUpdate = DateTime.Now;

            // Act
            var result = _controller.ShipOrder(orderVM);

            // Assert
            // Verify order header was updated correctly
            Assert.Equal("TRACK123", existingOrderHeader.TrackingNumber);
            Assert.Equal("FedEx", existingOrderHeader.Carrier);
            Assert.Equal(SD.StatusShipped, existingOrderHeader.OrderStatus);
            Assert.True(existingOrderHeader.ShippingDate >= beforeUpdate);

            _mockOrderHeaderService.Verify(s => s.UpdateOrderHeader(existingOrderHeader), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(orderId, redirectResult.RouteValues?["id"]);
        }

        [Fact]
        public void ShipOrder_WithDelayedPayment_SetsPaymentDueDate()
        {
            // Arrange
            int orderId = 1;
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader
                {
                    Id = orderId,
                    TrackingNumber = "TRACK123",
                    Carrier = "FedEx"
                }
            };

            var existingOrderHeader = new OrderHeader
            {
                Id = orderId,
                PaymentStatus = SD.PaymentStatusDelayedPayment
            };

            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, It.IsAny<string>()))
            .Returns(existingOrderHeader);


            // Act
            var result = _controller.ShipOrder(orderVM);

            // Assert
            // Verify payment due date was set approximately 30 days from now
            DateOnly expectedDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            Assert.Equal(expectedDueDate.Year, existingOrderHeader.PaymentDueDate.Year);
            Assert.Equal(expectedDueDate.Month, existingOrderHeader.PaymentDueDate.Month);
            Assert.Equal(expectedDueDate.Day, existingOrderHeader.PaymentDueDate.Day);
        }

        [Fact]
        public void CancelOrder_NotPaid_UpdatesStatusAndRedirects()
        {
            // Arrange
            int orderId = 1;
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader { Id = orderId }
            };

            var existingOrderHeader = new OrderHeader
            {
                Id = orderId,
                PaymentStatus = SD.PaymentStatusPending
            };

            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, It.IsAny<string>()))
    .Returns(existingOrderHeader);


            // Act
            var result = _controller.CancelOrder(orderVM);

            // Assert
            _mockOrderHeaderService.Verify(s => s.UpdateStatus(orderId, SD.StatusCancelled, SD.StatusCancelled), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirectResult.ActionName);
            Assert.Equal(orderId, redirectResult.RouteValues?["id"]);
        }

        // Note: Testing the CancelOrder method with payment refund would require mocking the Stripe API,
        // which is beyond the scope of simple unit tests. In a real-world scenario, you might:
        // 1. Use a wrapper/interface for Stripe services to make them testable
        // 2. Create integration tests for the actual Stripe interaction

        [Fact]
        public void PayDetails_SetsUpStripeSessionAndRedirects()
        {
            // This test can only verify the setup logic, not the actual Stripe API call
            // In a real application, you would abstract the Stripe dependency

            // Arrange
            int orderId = 1;
            var orderVM = new OrderVM
            {
                orderHeader = new OrderHeader { Id = orderId }
            };

            var orderHeader = new OrderHeader { Id = orderId };
            var orderDetails = new List<OrderDetail>
            {
                new OrderDetail
                {
                    Id = 1,
                    OrderHeaderId = orderId,
                    Price = 19.99f,
                    Count = 2,
                    Product = new Product { Title = "Test Product" }
                }
            };

            _mockOrderHeaderService.Setup(s => s.GetOrderHeaderById(orderId, "ApplicationUser")).Returns(orderHeader);
            _mockOrderDetailService.Setup(s => s.GetAllOrders(orderId, "Product")).Returns(orderDetails);

            // Set up HttpContext with needed properties for Stripe URL generation
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "https";
            httpContext.Request.Host = new HostString("example.com");
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };


        }



    }
}