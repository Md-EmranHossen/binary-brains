using ECommerceSystem.DataAccess.Repository;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace ECommerceSystem.Test.ServiceTests

{
    public class ShoppingCartServiceTests
    {
        private readonly Mock<IShoppingCartRepository> _mockShoppingCartRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly ShoppingCartService _shoppingCartService;
        private readonly string _testUserId = "test-user-id";

        public ShoppingCartServiceTests()
        {
            _mockShoppingCartRepo = new Mock<IShoppingCartRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();

            // Setup HTTP Context with mock session
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            var sessionDict = new Dictionary<string, byte[]>();

            mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => sessionDict[key] = value);
              mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny))
                .Returns<string, byte[]>((key, value) =>
                {
                    if (sessionDict.TryGetValue(key, out byte[] result))
                    {
                        value = result;
                        return true;
                    }
                    value = null!;
                    return false;
                });

            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            _shoppingCartService = new ShoppingCartService(
                _mockShoppingCartRepo.Object,
                _mockUnitOfWork.Object,
                _mockHttpContextAccessor.Object
            );
        }

        [Fact]
        public void AddShoppingCart_ShouldAddCartAndCommit()
        {
            // Arrange
            var cart = new ShoppingCart { Id = 1, ProductId = 1, Count = 1 };

            // Act
            _shoppingCartService.AddShoppingCart(cart);

            // Assert
            _mockShoppingCartRepo.Verify(r => r.Add(cart), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void DeleteShoppingCart_WhenCartExists_ShouldRemoveCartAndCommit()
        {
            // Arrange
            int cartId = 1;
            var cart = new ShoppingCart { Id = cartId, ProductId = 1, Count = 1 };
            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(cart);

            // Act
            _shoppingCartService.DeleteShoppingCart(cartId);

            // Assert
            _mockShoppingCartRepo.Verify(r => r.Remove(cart), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void DeleteShoppingCart_WhenCartDoesNotExist_ShouldNotRemoveOrCommit()
        {
            // Arrange
            int cartId = 1;
            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns((ShoppingCart?)null);

            // Act
            _shoppingCartService.DeleteShoppingCart(cartId);

            // Assert
            _mockShoppingCartRepo.Verify(r => r.Remove(It.IsAny<ShoppingCart>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void GetShoppingCartById_ShouldReturnCart()
        {
            // Arrange
            int cartId = 1;
            var expectedCart = new ShoppingCart { Id = cartId };
            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(expectedCart);

            // Act
            var result = _shoppingCartService.GetShoppingCartById(cartId);

            // Assert
            Assert.Equal(expectedCart, result);
        }

        [Fact]
        public void GetShoppingCartByUserAndProduct_ShouldReturnCart()
        {
            // Arrange
            string userId = "user123";
            int productId = 1;
            var expectedCart = new ShoppingCart { ApplicationUserId = userId, ProductId = productId };

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(expectedCart);

            // Act
            var result = _shoppingCartService.GetShoppingCartByUserAndProduct(userId, productId);

            // Assert
            Assert.Equal(expectedCart, result);
        }

        [Fact]
        public void UpdateShoppingCart_WhenCartExists_ShouldUpdateCountAndCommit()
        {
            // Arrange
            var cart = new ShoppingCart { Id = 1, Count = 5 };
            var existingCart = new ShoppingCart { Id = 1, Count = 1 };

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(existingCart);

            // Act
            _shoppingCartService.UpdateShoppingCart(cart);

            // Assert
            Assert.Equal(5, existingCart.Count);  // Verify count was updated
            _mockShoppingCartRepo.Verify(r => r.Update(existingCart), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateShoppingCart_WhenCartDoesNotExist_ShouldNotUpdateOrCommit()
        {
            // Arrange
            var cart = new ShoppingCart { Id = 1, Count = 5 };

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns((ShoppingCart?)null);

            // Act
            _shoppingCartService.UpdateShoppingCart(cart);

            // Assert
            _mockShoppingCartRepo.Verify(r => r.Update(It.IsAny<ShoppingCart>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void GetShoppingCartsByUserId_ShouldReturnUserCarts()
        {
            // Arrange
            string userId = "user123";
            var expectedCarts = new List<ShoppingCart>
            {
                new ShoppingCart { ApplicationUserId = userId, ProductId = 1 },
                new ShoppingCart { ApplicationUserId = userId, ProductId = 2 }
            };

            _mockShoppingCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>()))
                .Returns(expectedCarts);

            // Act
            var result = _shoppingCartService.GetShoppingCartsByUserId(userId);

            // Assert
            Assert.Equal(expectedCarts, result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void RemoveRange_ShouldRemoveCartsAndCommit()
        {
            // Arrange
            var carts = new List<ShoppingCart>
            {
                new ShoppingCart { Id = 1 },
                new ShoppingCart { Id = 2 }
            };

            // Act
            _shoppingCartService.RemoveRange(carts);

            // Assert
            _mockShoppingCartRepo.Verify(r => r.RemoveRange(carts), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void CreateCartWithProduct_ShouldReturnNewCart()
        {
            // Arrange
            var product = new Product { Id = 5, Title = "Test Product", Price = 10.99m };

            // Act
            var result = _shoppingCartService.CreateCartWithProduct(product);

            // Assert
            Assert.Equal(product, result.Product);
            Assert.Equal(product.Id, result.ProductId);
            Assert.Equal(1, result.Count);
        }





        [Fact]
        public void AddOrUpdateShoppingCart_WithInvalidParams_ShouldReturnFalse()
        {
            // Arrange - Invalid inputs
            string emptyUserId = "";
            var nullCart = (ShoppingCart?)null;
            var invalidProductCart = new ShoppingCart { ProductId = 0 };

            // Act & Assert
            Assert.False(_shoppingCartService.AddOrUpdateShoppingCart(invalidProductCart, _testUserId));
            Assert.False(_shoppingCartService.AddOrUpdateShoppingCart(nullCart!, _testUserId));
            Assert.False(_shoppingCartService.AddOrUpdateShoppingCart(new ShoppingCart { ProductId = 1 }, emptyUserId));
        }

        [Fact]
        public void GetShoppingCartVM_ShouldReturnViewModel()
        {
            // Arrange
            string userId = "user123";
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 10m },
                new Product { Id = 2, Title = "Product 2", Price = 20m }
            };

            var shoppingCarts = new List<ShoppingCart>
            {
                new ShoppingCart { ApplicationUserId = userId, ProductId = 1, Count = 2, Product = products[0] },
                new ShoppingCart { ApplicationUserId = userId, ProductId = 2, Count = 1, Product = products[1] }
            };

            _mockShoppingCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>()))
                .Returns(shoppingCarts);

            // Act
            var result = _shoppingCartService.GetShoppingCartVM(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(shoppingCarts, result.ShoppingCartList);
            Assert.Equal(40.0, result.OrderHeader.OrderTotal); // (10 * 2) + (20 * 1) = 40
        }

        [Fact]
        public void RemoveShoppingCarts_WithNullOrder_ShouldNotRemoveAnything()
        {
            // Arrange
            OrderHeader nullOrder = null!;

            // Act
            _shoppingCartService.RemoveShoppingCarts(nullOrder);

            // Assert
            _mockShoppingCartRepo.Verify(
                r => r.RemoveRange(It.IsAny<List<ShoppingCart>>()),
                Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }





        [Fact]
        public void GetShoppingCartByUserId_ShouldReturnCartsByUserId()
        {
            // Arrange
            string userId = "user123";
            var expectedCarts = new List<ShoppingCart>
            {
                new ShoppingCart { ApplicationUserId = userId, ProductId = 1 },
                new ShoppingCart { ApplicationUserId = userId, ProductId = 2 }
            };

            _mockShoppingCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>()))
                .Returns(expectedCarts);

            // Act
            var result = _shoppingCartService.GetShoppingCartByUserId(userId);

            // Assert
            Assert.Equal(expectedCarts, result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetShoppingCartVMForSummaryPost_ShouldReturnCorrectViewModel()
        {
            // Arrange
            string userId = "user123";
            var applicationUser = new ApplicationUser
            {
                Id = userId,
                Name = "Test User",
                PhoneNumber = "1234567890",
                StreetAddress = "123 Main St",
                City = "Test City",
                State = "TS",
                PostalCode = "12345",
                CompanyId = 0  // Regular user
            };

            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 10m },
                new Product { Id = 2, Title = "Product 2", Price = 20m }
            };

            var carts = new List<ShoppingCart>
            {
                new ShoppingCart { ApplicationUserId = userId, ProductId = 1, Count = 2, Product = products[0] },
                new ShoppingCart { ApplicationUserId = userId, ProductId = 2, Count = 1, Product = products[1] }
            };

            // Act
            var result = _shoppingCartService.GetShoppingCartVMForSummaryPost(carts, applicationUser, userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(carts, result.ShoppingCartList);
            Assert.Equal(40.0, result.OrderHeader.OrderTotal);  // (10 * 2) + (20 * 1) = 40
            Assert.Equal(userId, result.OrderHeader.ApplicationUserId);
            Assert.Equal("Test User", result.OrderHeader.Name);
            Assert.Equal(SD.PaymentStatusPending, result.OrderHeader.PaymentStatus);
            Assert.Equal(SD.StatusPending, result.OrderHeader.OrderStatus);

            // Check product prices were copied to cart items
            Assert.Equal(10.0, result.ShoppingCartList.First(c => c.ProductId == 1).Price);
            Assert.Equal(20.0, result.ShoppingCartList.First(c => c.ProductId == 2).Price);
        }

        [Fact]
        public void GetShoppingCartVMForSummaryPost_WithCompanyUser_ShouldSetDelayedPayment()
        {
            // Arrange
            string userId = "user123";
            var applicationUser = new ApplicationUser
            {
                Id = userId,
                Name = "Company User",
                CompanyId = 1  // Company user
            };

            var product = new Product { Id = 1, Title = "Product 1", Price = 10m };
            var carts = new List<ShoppingCart>
            {
                new ShoppingCart { ApplicationUserId = userId, ProductId = 1, Count = 1, Product = product }
            };

            // Act
            var result = _shoppingCartService.GetShoppingCartVMForSummaryPost(carts, applicationUser, userId);

            // Assert
            Assert.Equal(SD.PaymentStatusDelayedPayment, result.OrderHeader.PaymentStatus);
            Assert.Equal(SD.StatusApproved, result.OrderHeader.OrderStatus);
        }

        [Fact]
        public void CheckOutForUser_ShouldCreateStripeSession()
        {
            // Arrange
            var shoppingCartVM = new ShoppingCartVM
            {
                OrderHeader = new OrderHeader { Id = 1 },
                ShoppingCartList = new List<ShoppingCart>
                {
                    new ShoppingCart
                    {
                        Product = new Product { Id = 1, Title = "Product 1" },
                        Price = 10.0,
                        Count = 2
                    },
                    new ShoppingCart
                    {
                        Product = new Product { Id = 2, Title = "Product 2" },
                        Price = 20.0,
                        Count = 1
                    }
                }
            };

            // Act
            var result = _shoppingCartService.CheckOutForUser(shoppingCartVM);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("payment", result.Mode);
            Assert.Equal(2, result.LineItems.Count);

            // Check first line item
            var firstItem = result.LineItems[0];
            Assert.Equal(1000, firstItem.PriceData.UnitAmount);  // $10.00 = 1000 cents
            Assert.Equal("usd", firstItem.PriceData.Currency);
            Assert.Equal("Product 1", firstItem.PriceData.ProductData.Name);
            Assert.Equal(2, firstItem.Quantity);

            // Check second line item  
            var secondItem = result.LineItems[1];
            Assert.Equal(2000, secondItem.PriceData.UnitAmount);  // $20.00 = 2000 cents
            Assert.Equal("Product 2", secondItem.PriceData.ProductData.Name);
            Assert.Equal(1, secondItem.Quantity);

            // Check URLs
            Assert.Contains("OrderConfirmation?id=1", result.SuccessUrl);
            Assert.Contains("customer/cart/index", result.CancelUrl);
        }
    }
}