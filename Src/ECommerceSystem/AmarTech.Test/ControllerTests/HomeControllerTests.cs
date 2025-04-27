using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AmarTech.Application.Services;
using AmarTech.Application.Services.IServices;
using AmarTech.Domain.Entities;
using AmarTech.Web.Areas.Customer.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AmarTech.Test.ControllerTests
{
    public class HomeControllerTests
    {
        private readonly Mock<ILogger<HomeController>> _loggerMock;
        private readonly Mock<IProductService> _productServiceMock;
        private readonly Mock<IShoppingCartService> _shoppingCartServiceMock;
        private readonly HomeController _controller;

        public HomeControllerTests()
        {
            _loggerMock = new Mock<ILogger<HomeController>>();
            _productServiceMock = new Mock<IProductService>();
            _shoppingCartServiceMock = new Mock<IShoppingCartService>();

            _controller = new HomeController(
                _loggerMock.Object,
                _productServiceMock.Object,
                _shoppingCartServiceMock.Object);

            // Setup session
            var httpContext = new DefaultHttpContext();
            httpContext.Session = new Mock<ISession>().Object;
            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Setup TempData
            _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Assert
            Assert.NotNull(_controller);
        }

        [Fact]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new HomeController(
                    null,
                    _productServiceMock.Object,
                    _shoppingCartServiceMock.Object));

            Assert.Equal("logger", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullProductService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new HomeController(
                    _loggerMock.Object,
                    null,
                    _shoppingCartServiceMock.Object));

            Assert.Equal("productService", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullShoppingCartService_ThrowsArgumentNullException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new HomeController(
                    _loggerMock.Object,
                    _productServiceMock.Object,
                    null));

            Assert.Equal("shoppingCartService", ex.ParamName);
        }

        #endregion

        #region Index Action Tests

        [Fact]
        public void Index_WithValidModel_ReturnsViewWithProductList()
        {
            // Arrange
            int? page = 1;
            string query = null;
            var productList = new List<Product> { new Product(), new Product() };
            int totalCount = 10;
            int totalPages = 5;

            _productServiceMock.Setup(p => p.SkipAndTake(page, query))
                .Returns(productList);
            _productServiceMock.Setup(p => p.GetAllProductsCount(It.IsAny<string?>()))
                .Returns(totalCount);
            _productServiceMock.Setup(p => p.CalculateTotalPage(totalCount))
                .Returns(totalPages);

            // Setup user claim
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            _controller.HttpContext.Session = new Mock<ISession>().Object;

            _shoppingCartServiceMock.Setup(s => s.GetShoppingCartByUserId("test-user-id"))
                .Returns(new List<ShoppingCart> { new ShoppingCart() });

            // Act
            var result = _controller.Index(page, query);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            Assert.Equal(productList, model);
            Assert.Equal(1, _controller.ViewBag.CurrentPage);
            Assert.Equal(totalPages, _controller.ViewBag.TotalPages);
            Assert.Null(_controller.ViewBag.SearchQuery);
        }

        [Fact]
        public void Index_WithSearchQuery_ReturnsFilteredResults()
        {
            // Arrange
            int? page = 1;
            string query = "test";
            var productList = new List<Product> { new Product() };
            int totalCount = 1;
            int totalPages = 1;

            _productServiceMock.Setup(p => p.SkipAndTake(page, query))
                .Returns(productList);
            _productServiceMock.Setup(p => p.GetAllProductsCount(query))
                .Returns(totalCount);
            _productServiceMock.Setup(p => p.CalculateTotalPage(totalCount))
                .Returns(totalPages);

            // Act
            var result = _controller.Index(page, query);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            Assert.Equal(productList, model);
            Assert.Equal(query, _controller.ViewBag.SearchQuery);
        }

        [Fact]
        public void Index_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "Model error");

            // Act
            var result = _controller.Index(1, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Index_WithNullPage_DefaultsToPageOne()
        {
            // Arrange
            int? page = null;
            string query = null;
            var productList = new List<Product>();
            _productServiceMock.Setup(p => p.SkipAndTake(page, query))
                .Returns(productList);
            _productServiceMock.Setup(p => p.GetAllProductsCount(It.IsAny<string?>()))
                .Returns(0);
            _productServiceMock.Setup(p => p.CalculateTotalPage(0))
                .Returns(0);

            // Act
            var result = _controller.Index(page, query);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(1, _controller.ViewBag.CurrentPage);
        }

        #endregion

        #region Details Action Tests

        [Fact]
        public void Details_Get_WithValidProductId_ReturnsViewWithCart()
        {
            // Arrange
            int productId = 1;
            var product = new Product { Id = productId };
            var cart = new ShoppingCart { ProductId = productId };

            _productServiceMock.Setup(p => p.GetProductByIdwithCategory(productId))
                .Returns(product);
            _shoppingCartServiceMock.Setup(s => s.CreateCartWithProduct(product))
                .Returns(cart);

            // Act
            var result = _controller.Details(productId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ShoppingCart>(viewResult.Model);
            Assert.Equal(productId, model.ProductId);
        }

        [Fact]
        public void Details_Get_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "Model error");

            // Act
            var result = _controller.Details(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Details_Get_WithInvalidProductId_ReturnsBadRequest()
        {
            // Arrange
            int productId = 0;

            // Act
            var result = _controller.Details(productId);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid product ID.", badRequestResult.Value);
        }

        [Fact]
        public void Details_Get_WithNonExistentProductId_ReturnsNotFound()
        {
            // Arrange
            int productId = 99;
            _productServiceMock.Setup(p => p.GetProductByIdwithCategory(productId))
                .Returns((Product)null);

            // Act
            var result = _controller.Details(productId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Product not found", notFoundResult.Value);
        }

        [Fact]
        public void Details_Post_WithValidCartForAnonymousUser_RedirectsToIndex()
        {
            // Arrange
            var cart = new ShoppingCart { ProductId = 1, Count = 1 };

            // Act
            var result = _controller.Details(cart);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _shoppingCartServiceMock.Verify(s => s.AddToCart(cart), Times.Once);
        }

        [Fact]
        public void Details_Post_WithValidCartForAuthenticatedUser_RedirectsToIndex()
        {
            // Arrange
            var cart = new ShoppingCart { ProductId = 1, Count = 1 };

            // Setup user claim
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            }, "test"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            _controller.HttpContext.Session = new Mock<ISession>().Object;

            _shoppingCartServiceMock.Setup(s => s.AddOrUpdateShoppingCart(cart, "test-user-id"))
                .Returns(true);
            _shoppingCartServiceMock.Setup(s => s.GetShoppingCartByUserId("test-user-id"))
                .Returns(new List<ShoppingCart> { new ShoppingCart() });

            // Act
            var result = _controller.Details(cart);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            _shoppingCartServiceMock.Verify(s => s.AddOrUpdateShoppingCart(cart, "test-user-id"), Times.Once);
        }

        [Fact]
        public void Details_Post_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "Model error");

            // Act
            var result = _controller.Details(new ShoppingCart());

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Details_Post_WithNullCart_ReturnsBadRequest()
        {
            // Act
            var result = _controller.Details(null!);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid ShoppingCart Data", badRequestResult.Value);
        }

        [Fact]
        public void Details_Post_WithInvalidProductId_ReturnsBadRequest()
        {
            // Arrange
            var cart = new ShoppingCart { ProductId = 0 };

            // Act
            var result = _controller.Details(cart);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Invalid ShoppingCart Data", badRequestResult.Value);
        }

        [Fact]
        public void Details_Post_WithAuthenticatedUserAndFailedCartUpdate_ReturnsServerError()
        {
            // Arrange
            var cart = new ShoppingCart { ProductId = 1, Count = 1 };

            // Setup user claim
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "test-user-id"),
            }, "test"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            _shoppingCartServiceMock.Setup(s => s.AddOrUpdateShoppingCart(cart, "test-user-id"))
                .Returns(false);

            // Act
            var result = _controller.Details(cart);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Failed to update shopping cart.", statusCodeResult.Value);
        }

        #endregion

        #region Privacy Action Test

        [Fact]
        public void Privacy_ReturnsView()
        {
            // Act
            var result = _controller.Privacy();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        #endregion

        #region Error Action Test

        [Fact]
        public void Error_ReturnsViewWithErrorViewModel()
        {
            // Act
            var result = _controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.IsType<ErrorViewModel>(viewResult.Model);
        }

        #endregion
    }
}