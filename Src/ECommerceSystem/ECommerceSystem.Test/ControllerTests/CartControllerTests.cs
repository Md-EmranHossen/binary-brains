using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using ECommerceWebApp.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace ECommerceSystem.Test.ControllerTests
{
    public class CartControllerTests
    {
        private readonly Mock<IShoppingCartService> _mockShoppingCartService;
        private readonly Mock<IApplicationUserService> _mockApplicationUserService;
        private readonly Mock<IOrderHeaderService> _mockOrderHeaderService;
        private readonly Mock<IOrderDetailService> _mockOrderDetailService;
        private readonly CartController _controller;
        private readonly string _userId = "testUserId";

        public CartControllerTests()
        {
            _mockShoppingCartService = new Mock<IShoppingCartService>();
            _mockApplicationUserService = new Mock<IApplicationUserService>();
            _mockOrderHeaderService = new Mock<IOrderHeaderService>();
            _mockOrderDetailService = new Mock<IOrderDetailService>();

            _controller = new CartController(
                _mockShoppingCartService.Object,
                _mockOrderHeaderService.Object,
                _mockApplicationUserService.Object,
                _mockOrderDetailService.Object
            );

            // Setup controller context with user claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public void Controller_HasAreaAttribute()
        {
            // Arrange
            var controllerType = typeof(CartController);

            // Act
            var areaAttribute = controllerType.GetCustomAttribute<AreaAttribute>();

            // Assert
            Assert.NotNull(areaAttribute);
            Assert.Equal("customer", areaAttribute.RouteValue);
        }

        [Fact]
        public void Controller_HasAuthorizeAttribute()
        {
            // Arrange
            var controllerType = typeof(CartController);

            // Act
            var authorizeAttribute = controllerType.GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.NotNull(authorizeAttribute);
        }

        [Fact]
        public void Index_ReturnsViewResult_WithShoppingCartVM()
        {
            // Arrange
            var shoppingCartVM = new ShoppingCartVM();
            _mockShoppingCartService.Setup(s => s.GetShoppingCartVM(_userId)).Returns(shoppingCartVM);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ShoppingCartVM>(viewResult.Model);
            Assert.Same(shoppingCartVM, model);
        }

        [Fact]
        public void Summary_WhenUserIdentityIsNull_ReturnsUnauthorized()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal() }
            };

            // Act
            var result = _controller.Summary();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void Summary_WhenUserIdNotFound_ReturnsUnauthorized()
        {
            // Arrange
            var claims = new List<Claim>();  // No NameIdentifier claim
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = _controller.Summary();

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void Summary_WhenShoppingCartVMIsNull_ReturnsNotFound()
        {
            // Arrange
            _mockShoppingCartService.Setup(s => s.GetShoppingCartVM(_userId)).Returns((ShoppingCartVM)null);

            // Act
            var result = _controller.Summary();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Summary_WhenUserNotFound_ReturnsNotFound()
        {
            // Arrange
            var shoppingCartVM = new ShoppingCartVM();
            _mockShoppingCartService.Setup(s => s.GetShoppingCartVM(_userId)).Returns(shoppingCartVM);
            _mockApplicationUserService.Setup(s => s.GetUserById(_userId)).Returns((ApplicationUser?)null);

            // Act
            var result = _controller.Summary();

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Summary_ValidRequest_ReturnsViewWithModel()
        {
            // Arrange
            var shoppingCartVM = new ShoppingCartVM
            {
                OrderHeader = new OrderHeader(),
                ShoppingCartList = new List<ShoppingCart>
                {
                    new ShoppingCart { Product = new Product { Price = 10.99m } }
                }
            };
            var user = new ApplicationUser
            {
                Name = "Test User",
                PhoneNumber = "1234567890",
                StreetAddress = "123 Test St",
                City = "Test City",
                State = "TS",
                PostalCode = "12345"
            };

            _mockShoppingCartService.Setup(s => s.GetShoppingCartVM(_userId)).Returns(shoppingCartVM);
            _mockApplicationUserService.Setup(s => s.GetUserById(_userId)).Returns(user);

            // Act
            var result = _controller.Summary();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<ShoppingCartVM>(viewResult.Model);
            Assert.Same(shoppingCartVM, model);
            Assert.Same(user, model.OrderHeader.ApplicationUser);
            Assert.Equal(user.Name, model.OrderHeader.Name);
            Assert.Equal(user.PhoneNumber, model.OrderHeader.PhoneNumber);
            Assert.Equal(user.StreetAddress, model.OrderHeader.StreetAddress);
            Assert.Equal(user.City, model.OrderHeader.City);
            Assert.Equal(user.State, model.OrderHeader.State);
            Assert.Equal(user.PostalCode, model.OrderHeader.PostalCode);
        }

        [Fact]
        public void SummaryPost_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");
            var shoppingCartVM = new ShoppingCartVM();

            // Act
            var result = _controller.SummaryPost(shoppingCartVM);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void SummaryPost_NullUserId_ReturnsUnauthorized()
        {
            // Arrange
            var claims = new List<Claim>();  // No NameIdentifier claim
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var shoppingCartVM = new ShoppingCartVM();

            // Act
            var result = _controller.SummaryPost(shoppingCartVM);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void SummaryPost_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            var shoppingCartList = new List<ShoppingCart>();
            _mockShoppingCartService.Setup(s => s.GetShoppingCartsByUserId(_userId)).Returns(shoppingCartList);
            _mockApplicationUserService.Setup(s => s.GetUserById(_userId)).Returns((ApplicationUser?)null);
            var shoppingCartVM = new ShoppingCartVM();

            // Act
            var result = _controller.SummaryPost(shoppingCartVM);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("User not found.", notFoundResult.Value);
        }

        [Fact]
        public void SummaryPost_ForCompanyUser_RedirectsToOrderConfirmation()
        {
            // Arrange
            var shoppingCartList = new List<ShoppingCart>();
            var user = new ApplicationUser { CompanyId = 1, Name = "Rifat" }; // CompanyId is not 0
            var returnedShoppingCartVM = new ShoppingCartVM
            {
                OrderHeader = new OrderHeader { Id = 123 }
            };

            _mockShoppingCartService.Setup(s => s.GetShoppingCartsByUserId(_userId)).Returns(shoppingCartList);
            _mockApplicationUserService.Setup(s => s.GetUserById(_userId)).Returns(user);
            _mockShoppingCartService.Setup(s => s.GetShoppingCartVMForSummaryPost(shoppingCartList, user, _userId))
                .Returns(returnedShoppingCartVM);

            var inputShoppingCartVM = new ShoppingCartVM();

            // Act
            var result = _controller.SummaryPost(inputShoppingCartVM);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("OrderConfirmation", redirectResult.ActionName);
            Assert.Equal(123, redirectResult.RouteValues["id"]);
        }


        [Fact]
        public void OrderConfirmation_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.OrderConfirmation(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void OrderConfirmation_ValidRequest_ReturnsViewWithId()
        {
            // Arrange
            var orderHeader = new OrderHeader { Id = 123 };
            _mockOrderHeaderService.Setup(s => s.OrderConfirmation(123)).Returns(orderHeader);

            // Act
            var result = _controller.OrderConfirmation(123);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(123, viewResult.Model);
        }

        [Fact]
        public void Plus_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.Plus(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Plus_ValidRequest_RedirectsToIndex()
        {
            // Act
            var result = _controller.Plus(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify service was called
            _mockShoppingCartService.Verify(s => s.Plus(1), Times.Once);
        }

        [Fact]
        public void Minus_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.Minus(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Minus_ValidRequest_RedirectsToIndex()
        {
            // Act
            var result = _controller.Minus(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify service was called
            _mockShoppingCartService.Verify(s => s.Minus(1), Times.Once);
        }

        [Fact]
        public void Remove_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.Remove(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Remove_ValidRequest_RedirectsToIndex()
        {
            // Act
            var result = _controller.Remove(1);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Verify service was called
            _mockShoppingCartService.Verify(s => s.RemoveCartValue(1), Times.Once);
        }
    }
}