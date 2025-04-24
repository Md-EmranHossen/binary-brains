using AmarTech.Domain.Entities;
using ECommerceSystem.Service.Services.IServices;
using AmarTech.Web.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using Xunit;

namespace ECommerceSystem.Test.ControllerTests
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _mockLogger;
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IShoppingCartService> _mockShoppingCartService;
        private readonly HomeController _controller;
        private readonly string _userId = "testUserId";

        public HomeControllerTests()
        {
            _mockLogger = new Mock<ILogger<HomeController>>();
            _mockProductService = new Mock<IProductService>();
            _mockShoppingCartService = new Mock<IShoppingCartService>();

            _controller = new HomeController(
                _mockLogger.Object,
                _mockProductService.Object,
                _mockShoppingCartService.Object
            );

            // Set up session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new MockHttpSession();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public void Constructor_NullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new HomeController(
                null!,
                _mockProductService.Object,
                _mockShoppingCartService.Object));

            Assert.Equal("logger", exception.ParamName);
        }

        [Fact]
        public void Constructor_NullProductService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new HomeController(
                _mockLogger.Object,
                null!,
                _mockShoppingCartService.Object));

            Assert.Equal("productService", exception.ParamName);
        }

        [Fact]
        public void Constructor_NullShoppingCartService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => new HomeController(
                _mockLogger.Object,
                _mockProductService.Object,
                null!));

            Assert.Equal("shoppingCartService", exception.ParamName);
        }

        [Fact]
        public void Controller_HasAreaAttribute()
        {
            // Arrange
            var controllerType = typeof(HomeController);

            // Act
            var areaAttribute = controllerType.GetCustomAttribute<AreaAttribute>();

            // Assert
            Assert.NotNull(areaAttribute);
            Assert.Equal("Customer", areaAttribute.RouteValue);
        }

        [Fact]
        public void Index_ReturnsViewResult_WithProductList()
        {
            // Arrange
            var productList = new List<Product> { new Product(), new Product() };
            var shoppingCartList = new List<ShoppingCart> { new ShoppingCart(), new ShoppingCart() };

            _mockProductService.Setup(s => s.GetAllProducts()).Returns(productList);
            _mockShoppingCartService.Setup(s => s.GetShoppingCartByUserId(It.IsAny<string>())).Returns(shoppingCartList);

            // Act
            var result = _controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);

            // Compare contents instead of reference
            Assert.Equal(productList.Count, model.Count());
            foreach (var (expected, actual) in productList.Zip(model))
            {
                Assert.Equal(expected.Id, actual.Id); // Adjust properties as needed
                                                      // Add other property comparisons
            }

            // Check that session was updated
            Assert.Equal(2, _controller.HttpContext.Session.GetInt32(SD.SessionCart));
        }

        [Fact]
        public void Index_WithAuthenticatedUser_SetsShoppingCartCount()
        {
            // Arrange
            var productList = new List<Product> { new Product(), new Product() };
            var shoppingCartList = new List<ShoppingCart> { new ShoppingCart(), new ShoppingCart(), new ShoppingCart() };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _userId) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext.HttpContext.User = principal;

            _mockProductService.Setup(s => s.GetAllProducts()).Returns(productList);
            _mockShoppingCartService.Setup(s => s.GetShoppingCartByUserId(_userId)).Returns(shoppingCartList);

            // Act
            var result = _controller.Index(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);

            // Check that session was updated with correct count
            Assert.Equal(3, _controller.HttpContext.Session.GetInt32(SD.SessionCart));
        }

        [Fact]
        public void Details_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.Details(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Details_InvalidProductId_ReturnsBadRequest()
        {
            // Arrange - No setup needed

            // Act
            var result = _controller.Details(0);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid product ID.", badRequestResult.Value);
        }

        [Fact]
        public void Details_ProductNotFound_ReturnsNotFound()
        {
            // Arrange
            _mockProductService.Setup(s => s.GetProductByIdwithCategory(1)).Returns((Product?)null);

            // Act
            var result = _controller.Details(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Product not found", notFoundResult.Value);
        }

        [Fact]
        public void Details_ValidProduct_ReturnsViewWithShoppingCart()
        {
            // Arrange
            var product = new Product { Id = 1, Title = "Test Product" };
            var cart = new ShoppingCart { Product = product, Count = 1 };

            _mockProductService.Setup(s => s.GetProductByIdwithCategory(1)).Returns(product);
            _mockShoppingCartService.Setup(s => s.CreateCartWithProduct(product)).Returns(cart);

            // Act
            var result = _controller.Details(1);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ShoppingCart>(viewResult.Model);
            Assert.Same(cart, model);
        }

        [Fact]
        public void DetailsPost_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");
            var shoppingCart = new ShoppingCart();

            // Act
            var result = _controller.Details(shoppingCart);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void DetailsPost_NullShoppingCart_ReturnsBadRequest()
        {
            // Arrange - No setup needed

            // Act
            var result = _controller.Details(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid ShoppingCart Data", badRequestResult.Value);
        }

        [Fact]
        public void DetailsPost_InvalidProductId_ReturnsBadRequest()
        {
            // Arrange
            var shoppingCart = new ShoppingCart { ProductId = 0 };

            // Act
            var result = _controller.Details(shoppingCart);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid ShoppingCart Data", badRequestResult.Value);
        }

        [Fact]
        public void DetailsPost_NoUserId_ReturnsUnauthorized()
        {
            // Arrange
            var shoppingCart = new ShoppingCart { ProductId = 1, Count = 1 };

            // No user claims set up, default is null

            // Act
            var result = _controller.Details(shoppingCart);

            // Assert
            Assert.IsType<UnauthorizedResult>(result);
        }

        [Fact]
        public void DetailsPost_AddToCartFails_Returns500()
        {
            // Arrange
            var shoppingCart = new ShoppingCart { ProductId = 1, Count = 1 };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _userId) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext.HttpContext.User = principal;

            _mockShoppingCartService.Setup(s => s.AddOrUpdateShoppingCart(shoppingCart, _userId)).Returns(false);

            // Act
            var result = _controller.Details(shoppingCart);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Failed to update shopping cart.", statusCodeResult.Value);
        }

        [Fact]
        public void DetailsPost_Success_RedirectsToIndex()
        {
            // Arrange
            var shoppingCart = new ShoppingCart { ProductId = 1, Count = 1 };
            var cartList = new List<ShoppingCart> { new ShoppingCart(), new ShoppingCart() };

            var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, _userId) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext.HttpContext.User = principal;

            _mockShoppingCartService.Setup(s => s.AddOrUpdateShoppingCart(shoppingCart, _userId)).Returns(true);
            _mockShoppingCartService.Setup(s => s.GetShoppingCartByUserId(_userId)).Returns(cartList);

            // Act
            var result = _controller.Details(shoppingCart);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);

            // Check that session was updated
            Assert.Equal(2, _controller.HttpContext.Session.GetInt32(SD.SessionCart));
        }

        [Fact]
        public void Privacy_ReturnsViewResult()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_ReturnsViewWithErrorViewModel()
        {
            // Arrange - Set trace identifier in HttpContext
            _controller.HttpContext.TraceIdentifier = "test-trace-id";

            // Act
            var result = _controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.Equal("test-trace-id", model.RequestId);
        }

        [Fact]
        public void DetailsPost_HasAuthorizeAttribute()
        {
            // Arrange
            var methodInfo = typeof(HomeController).GetMethod("Details", new[] { typeof(ShoppingCart) });

            // Act
            var attribute = methodInfo?.GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.NotNull(attribute);
        }

        [Fact]
        public void DetailsPost_HasValidateAntiForgeryTokenAttribute()
        {
            // Arrange
            var methodInfo = typeof(HomeController).GetMethod("Details", new[] { typeof(ShoppingCart) });

            // Act
            var attribute = methodInfo?.GetCustomAttribute<ValidateAntiForgeryTokenAttribute>();

            // Assert
            Assert.NotNull(attribute);
        }
    }

    // Mock Http Session for testing
    public class MockHttpSession : ISession
    {
        private readonly Dictionary<string, object> _sessionStorage = new Dictionary<string, object>();

        public string Id => "testId";
        public bool IsAvailable => true;
        public IEnumerable<string> Keys => _sessionStorage.Keys;

        public void Clear()
        {
            _sessionStorage.Clear();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public void Remove(string key)
        {
            _sessionStorage.Remove(key);
        }

        public void Set(string key, byte[] value)
        {
            _sessionStorage[key] = value;
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            if (_sessionStorage.TryGetValue(key, out var objectValue) && objectValue is byte[] byteArray)
            {
                value = byteArray;
                return true;
            }

            value = null!;
            return false;
        }
    }
}