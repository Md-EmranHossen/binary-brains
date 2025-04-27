using AmarTech.Domain.Entities;
using AmarTech.Infrastructure.Repository;
using AmarTech.Infrastructure.Repository.IRepository;
using AmarTech.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace AmarTech.Test.ServiceTests
{
    public class ShoppingCartServiceTests
    {
       /* private readonly Mock<IShoppingCartRepository> _mockShoppingCartRepo;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<IMemoryCache> _mockMemoryCache;
        private readonly ShoppingCartService _shoppingCartService;
        private readonly string _testUserId = "test-user-id";
        private readonly string _guestCartKey = "guest_cart";

        public ShoppingCartServiceTests()
        {
            _mockShoppingCartRepo = new Mock<IShoppingCartRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            // Setup HTTP Context with mock session
            var mockHttpContext = new Mock<HttpContext>();
            var mockSession = new Mock<ISession>();
            var sessionDict = new Dictionary<string, byte[]>();

            mockSession.Setup(s => s.Set(It.IsAny<string>(), It.IsAny<byte[]>()))
                .Callback<string, byte[]>((key, value) => sessionDict[key] = value);
            mockSession.Setup(s => s.TryGetValue(It.IsAny<string>(), out It.Ref<byte[]>.IsAny!))
                .Returns<string, byte[]>((key, value) =>
                {
                    if (sessionDict.TryGetValue(key, out byte[]? result))
                    {
                        value = result;
                        return true;
                    }
                    value = null!;
                    return false;
                });
            mockSession.Setup(s => s.SetInt32(It.IsAny<string>(), It.IsAny<int>()))
                .Callback<string, int>((key, value) => { *//* do nothing *//* });

            mockHttpContext.Setup(c => c.Session).Returns(mockSession.Object);
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockHttpContext.Object);

            // Setup Memory Cache
            var cacheEntryMock = new Mock<ICacheEntry>();
            _mockMemoryCache.Setup(m => m.CreateEntry(It.IsAny<object>()))
                .Returns(cacheEntryMock.Object);

            _mockMemoryCache.Setup(m => m.Get<List<ShoppingCart>>(_guestCartKey))
                .Returns((List<ShoppingCart>)null);

            _shoppingCartService = new ShoppingCartService(
                _mockShoppingCartRepo.Object,
                _mockUnitOfWork.Object,
                _mockHttpContextAccessor.Object,
                _mockMemoryCache.Object
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
        public void DeleteShoppingCart_WithNullId_ShouldNotRemoveOrCommit()
        {
            // Arrange
            int? cartId = null;

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
        public void GetShoppingCartById_WithTracking_ShouldReturnCart()
        {
            // Arrange
            int cartId = 1;
            bool track = true;
            var expectedCart = new ShoppingCart { Id = cartId };
            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(expectedCart);

            // Act
            var result = _shoppingCartService.GetShoppingCartById(cartId, track);

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
        public void GetShoppingCartsByUserId_WhenNoCartsExist_ShouldReturnEmptyList()
        {
            // Arrange
            string userId = "user123";
            _mockShoppingCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>()))
                .Returns((IEnumerable<ShoppingCart>)null);

            // Act
            var result = _shoppingCartService.GetShoppingCartsByUserId(userId);

            // Assert
            Assert.Empty(result);
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
        public void AddOrUpdateShoppingCart_WithExistingCart_ShouldUpdateCountAndCommit()
        {
            // Arrange
            string userId = "user123";
            int productId = 1;
            var existingCart = new ShoppingCart { Id = 1, ApplicationUserId = userId, ProductId = productId, Count = 1 };
            var newCart = new ShoppingCart { ProductId = productId, Count = 2 };

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(existingCart);

            // Act
            bool result = _shoppingCartService.AddOrUpdateShoppingCart(newCart, userId);

            // Assert
            Assert.True(result);
            Assert.Equal(3, existingCart.Count);  // 1 (existing) + 2 (new) = 3
            _mockShoppingCartRepo.Verify(r => r.Update(existingCart), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void AddOrUpdateShoppingCart_WithNewProduct_ShouldAddNewCartAndCommit()
        {
            // Arrange
            string userId = "user123";
            int productId = 1;
            var newCart = new ShoppingCart { ProductId = productId, Count = 1 };

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns((ShoppingCart?)null);

            // Act
            bool result = _shoppingCartService.AddOrUpdateShoppingCart(newCart, userId);

            // Assert
            Assert.True(result);
            Assert.Equal(userId, newCart.ApplicationUserId);
            _mockShoppingCartRepo.Verify(r => r.Add(newCart), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
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
            Assert.Equal(40.0, result.OrderHeader.OrderTotal); 
        }

        [Fact]
        public void GetShoppingCartVM_WithProductDiscounts_ShouldCalculateCorrectTotal()
        {
            // Arrange
            string userId = "user123";
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 10m, DiscountAmount = 2m },
                new Product { Id = 2, Title = "Product 2", Price = 20m, DiscountAmount = 5m }
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
            // (10-2)*2 + (20-5)*1 = 16 + 15 = 31
            Assert.Equal(31.0, result.OrderHeader.OrderTotal);
        }

        [Fact]
        public void GetShoppingCartVM_WithNullProducts_ShouldHandleGracefully()
        {
            // Arrange
            string userId = "user123";
            var shoppingCarts = new List<ShoppingCart>
            {
                new ShoppingCart { ApplicationUserId = userId, ProductId = 1, Count = 2, Product = null },
                new ShoppingCart { ApplicationUserId = userId, ProductId = 2, Count = 1, Product = new Product { Price = 20m } }
            };

            _mockShoppingCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>()))
                .Returns(shoppingCarts);

            // Act
            var result = _shoppingCartService.GetShoppingCartVM(userId);

            // Assert
            Assert.NotNull(result);
            // Only the valid product should be counted: 20 * 1 = 20
            Assert.Equal(20.0, result.OrderHeader.OrderTotal);
        }

        [Fact]
        public void RemoveShoppingCarts_WithValidOrder_ShouldRemoveCartsAndCommit()
        {
            // Arrange
            string userId = "user123";
            var orderHeader = new OrderHeader { ApplicationUserId = userId };
            var carts = new List<ShoppingCart>
            {
                new ShoppingCart { ApplicationUserId = userId, ProductId = 1 },
                new ShoppingCart { ApplicationUserId = userId, ProductId = 2 }
            };

            _mockShoppingCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>()))
                .Returns(carts);

            // Act
            var result = _shoppingCartService.RemoveShoppingCarts(orderHeader);

            // Assert
            Assert.Equal(carts, result);
            _mockShoppingCartRepo.Verify(r => r.RemoveRange(carts), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void RemoveShoppingCarts_WithNullOrder_ShouldReturnEmptyList()
        {
            // Arrange
            OrderHeader? nullOrder = null;

            // Act
            var result = _shoppingCartService.RemoveShoppingCarts(nullOrder);

            // Assert
            Assert.Empty(result);
            _mockShoppingCartRepo.Verify(r => r.RemoveRange(It.IsAny<List<ShoppingCart>>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void Plus_WithDbCart_ShouldIncrementCount()
        {
            // Arrange
            int cartId = 1;
            var cart = new ShoppingCart
            {
                Id = cartId,
                Count = 1,
                Product = new Product { StockQuantity = 10 }
            };

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(cart);

            // Act
            _shoppingCartService.Plus(cart, cartId);

            // Assert
            Assert.Equal(2, cart.Count);
            _mockShoppingCartRepo.Verify(r => r.Update(cart), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void Plus_WithDbCart_ExceedingStock_ShouldNotIncrement()
        {
            // Arrange
            int cartId = 1;
            var cart = new ShoppingCart
            {
                Id = cartId,
                Count = 10,
                Product = new Product { StockQuantity = 10 }
            };

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(cart);

            // Act
            _shoppingCartService.Plus(cart, cartId);

            // Assert
            Assert.Equal(10, cart.Count); // Count remains unchanged
            _mockShoppingCartRepo.Verify(r => r.Update(It.IsAny<ShoppingCart>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void Plus_WithMemoryCart_ShouldIncrementCount()
        {
            // Arrange
            int cartId = 1;
            var memoryCart = new List<ShoppingCart>
            {
                new ShoppingCart
                {
                    Id = cartId,
                    Count = 1,
                    Product = new Product { StockQuantity = 10 }
                }
            };

            _mockMemoryCache.Setup(m => m.Get<List<ShoppingCart>>(_guestCartKey))
                .Returns(memoryCart);

            // Act
            _shoppingCartService.Plus(null, cartId);

            // Assert
            Assert.Equal(2, memoryCart[0].Count);
            _mockMemoryCache.Verify(m => m.Set(_guestCartKey, memoryCart, It.IsAny<MemoryCacheEntryOptions>()), Times.AtLeastOnce);
        }

        [Fact]
        public void Plus_WithMemoryCart_ExceedingStock_ShouldNotIncrement()
        {
            // Arrange
            int cartId = 1;
            var memoryCart = new List<ShoppingCart>
            {
                new ShoppingCart
                {
                    Id = cartId,
                    Count = 10,
                    Product = new Product { StockQuantity = 10 }
                }
            };

            _mockMemoryCache.Setup(m => m.Get<List<ShoppingCart>>(_guestCartKey))
                .Returns(memoryCart);

            // Act
            _shoppingCartService.Plus(null, cartId);

            // Assert
            Assert.Equal(10, memoryCart[0].Count); // Count remains unchanged
        }

        [Fact]
        public void Minus_WithDbCart_ShouldDecrementCount()
        {
            // Arrange
            int cartId = 1;
            string userId = "test-user";
            var cart = new ShoppingCart
            {
                Id = cartId,
                Count = 2,
                ApplicationUserId = userId
            };

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(cart);

            _mockShoppingCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>()))
                .Returns(new List<ShoppingCart> { cart });

            // Act
            _shoppingCartService.Minus(cartId);

            // Assert
            Assert.Equal(1, cart.Count);
            _mockShoppingCartRepo.Verify(r => r.Update(cart), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void Minus_WithDbCart_CountIsOne_ShouldRemoveCart()
        {
            // Arrange
            int cartId = 1;
            string userId = "test-user";
            var cart = new ShoppingCart
            {
                Id = cartId,
                Count = 1,
                ApplicationUserId = userId
            };

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(cart);

            _mockShoppingCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>()))
                .Returns(new List<ShoppingCart> { cart });

            // Act
            _shoppingCartService.Minus(cartId);

            // Assert
            _mockShoppingCartRepo.Verify(r => r.Remove(cart), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void Minus_WithMemoryCart_ShouldDecrementCount()
        {
            // Arrange
            int cartId = 1;
            var memoryCart = new List<ShoppingCart>
            {
                new ShoppingCart
                {
                    Id = cartId,
                    Count = 2
                }
            };

            _mockMemoryCache.Setup(m => m.Get<List<ShoppingCart>>(_guestCartKey))
                .Returns(memoryCart);

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns((ShoppingCart?)null);

            // Act
            _shoppingCartService.Minus(cartId);

            // Assert
            Assert.Equal(1, memoryCart[0].Count);
            _mockMemoryCache.Verify(m => m.Set(_guestCartKey, memoryCart, It.IsAny<MemoryCacheEntryOptions>()), Times.AtLeastOnce);
        }

        [Fact]
        public void Minus_WithMemoryCart_CountIsOne_ShouldRemoveCart()
        {
            // Arrange
            int cartId = 1;
            var memoryCart = new List<ShoppingCart>
            {
                new ShoppingCart
                {
                    Id = cartId,
                    Count = 1
                }
            };

            _mockMemoryCache.Setup(m => m.Get<List<ShoppingCart>>(_guestCartKey))
                .Returns(memoryCart);

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns((ShoppingCart?)null);

            // Act
            _shoppingCartService.Minus(cartId);

            // Assert
            Assert.Empty(memoryCart);
            _mockMemoryCache.Verify(m => m.Set(_guestCartKey, memoryCart, It.IsAny<MemoryCacheEntryOptions>()), Times.AtLeastOnce);
        }

        [Fact]
        public void RemoveCartValue_WithDbCart_ShouldRemoveCart()
        {
            // Arrange
            int cartId = 1;
            string userId = "test-user";
            var cart = new ShoppingCart
            {
                Id = cartId,
                ApplicationUserId = userId
            };

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns(cart);

            _mockShoppingCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>()))
                .Returns(new List<ShoppingCart> { cart });

            // Act
            _shoppingCartService.RemoveCartValue(cartId);

            // Assert
            _mockShoppingCartRepo.Verify(r => r.Remove(cart), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void RemoveCartValue_WithMemoryCart_ShouldRemoveCart()
        {
            // Arrange
            int cartId = 1;
            var memoryCart = new List<ShoppingCart>
            {
                new ShoppingCart { Id = cartId }
            };

            _mockMemoryCache.Setup(m => m.Get<List<ShoppingCart>>(_guestCartKey))
                .Returns(memoryCart);

            _mockShoppingCartRepo.Setup(r => r.Get(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
                .Returns((ShoppingCart?)null);

            // Act
            _shoppingCartService.RemoveCartValue(cartId);

            // Assert
            Assert.Empty(memoryCart);
            _mockMemoryCache.Verify(m => m.Set(_guestCartKey, memoryCart, It.IsAny<MemoryCacheEntryOptions>()), Times.AtLeastOnce);
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
        public void GetShoppingCartVMForSummaryPost_WithProductDiscounts_ShouldCalculateDiscountedPrices()
        {
            // Arrange
            string userId = "user123";
            var applicationUser = new ApplicationUser { Id = userId, Name = "Test User" };

            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 10m, DiscountAmount = 2m },
                new Product { Id = 2, Title = "Product 2", Price = 20m, DiscountAmount = 5m }
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
            // (10-2)*2 + (20-5)*1 = 16 + 15 = 31
            Assert.Equal(31.0, result.OrderHeader.OrderTotal);
            Assert.Equal(8.0, result.ShoppingCartList.First(c => c.ProductId == 1).Price); // 10 - 2 = 8
            Assert.Equal(15.0, result.ShoppingCartList.First(c => c.ProductId == 2).Price); // 20 - 5 = 15
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

        [Fact]
        public void CheckOutForUser_WithNullProductInCart_ShouldSkipThatItem()
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
                        Product = null, // Null product should be skipped
                        Price = 20.0,
                        Count = 1
                    }
                }
            };

            // Act
            var result = _shoppingCartService.CheckOutForUser(shoppingCartVM);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.LineItems); // Only one valid item
        }

        [Fact]
        public void AddToCart_ShouldAddCartToMemoryCache()
        {
            // Arrange
            var cart = new ShoppingCart { Id = 1, ProductId = 1, Count = 1 };
            var existingCarts = new List<ShoppingCart>();

            _mockMemoryCache.Setup(m => m.Get<List<ShoppingCart>>(_guestCartKey))
                .Returns(existingCarts);

            // Act
            _shoppingCartService.AddToCart(cart);

            // Assert
            Assert.Single(existingCarts);
            Assert.Equal(cart, existingCarts[0]);
            _mockMemoryCache.Verify(m => m.Set(_guestCartKey, existingCarts, It.IsAny<MemoryCacheEntryOptions>()), Times.AtLeastOnce);
        }

        [Fact]
        public void GetCart_WhenCacheHasValue_ShouldReturnCachedValue()
        {
            // Arrange
            var expectedCarts = new List<ShoppingCart>
            {
                new ShoppingCart { Id = 1, ProductId = 1 }
            };

            _mockMemoryCache.Setup(m => m.Get<List<ShoppingCart>>(_guestCartKey))
                .Returns(expectedCarts);

            // Act
            var result = _shoppingCartService.GetCart();

            // Assert
            Assert.Equal(expectedCarts, result);
            Assert.Single(result);
        }

        [Fact]
        public void GetCart_WhenCacheIsEmpty_ShouldReturnEmptyList()
        {
            // Arrange
            _mockMemoryCache.Setup(m => m.Get<List<ShoppingCart>>(_guestCartKey))
                .Returns((List<ShoppingCart>)null);

            // Act
            var result = _shoppingCartService.GetCart();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ClearCart_ShouldRemoveFromMemoryCache()
        {
            // Act
            _shoppingCartService.ClearCart();

            // Assert
            _mockMemoryCache.Verify(m => m.Remove(_guestCartKey), Times.Once);
        }

        [Fact]
        public void SetInMemory_WithValidCart_ShouldSetInMemoryCache()
        {
            // Arrange
            var carts = new List<ShoppingCart> { new ShoppingCart { Id = 1 } };

            // Act
            _shoppingCartService.SetInMemory(carts);

            // Assert
            _mockMemoryCache.Verify(m => m.Set(_guestCartKey, carts, It.IsAny<MemoryCacheEntryOptions>()), Times.Once);
        }

        [Fact]
        public void SetInMemory_WithNullCart_ShouldNotSetInMemoryCache()
        {
            // Act
            _shoppingCartService.SetInMemory(null);

            // Assert
            _mockMemoryCache.Verify(m => m.Set(
                It.IsAny<string>(),
                It.IsAny<List<ShoppingCart>>(),
                It.IsAny<MemoryCacheEntryOptions>()),
                Times.Never);
        }

        [Fact]
        public void MemoryCartVM_ShouldReturnShoppingCartVM()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 10m },
                new Product { Id = 2, Title = "Product 2", Price = 20m }
            };

            var carts = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 1, Count = 2, Product = products[0] },
                new ShoppingCart { ProductId = 2, Count = 1, Product = products[1] }
            };

            // Act
            var result = _shoppingCartService.MemoryCartVM(carts);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(carts, result.ShoppingCartList);
            Assert.Equal(40.0, result.OrderHeader.OrderTotal); // (10 * 2) + (20 * 1) = 40
        }

        [Fact]
        public void MemoryCartVM_WithDiscounts_ShouldCalculateCorrectTotal()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 10m, DiscountAmount = 2m },
                new Product { Id = 2, Title = "Product 2", Price = 20m, DiscountAmount = 5m }
            };

            var carts = new List<ShoppingCart>
            {
                new ShoppingCart { ProductId = 1, Count = 2, Product = products[0] },
                new ShoppingCart { ProductId = 2, Count = 1, Product = products[1] }
            };

            // Act
            var result = _shoppingCartService.MemoryCartVM(carts);

            // Assert
            Assert.NotNull(result);
            // (10-2)*2 + (20-5)*1 = 16 + 15 = 31
            Assert.Equal(31.0, result.OrderHeader.OrderTotal);
        }

        [Fact]
        public void CombineToDB_ShouldCombineCartsAndClearCache()
        {
            // Arrange
            string userId = "user123";
            var dbCarts = new List<ShoppingCart>
            {
                new ShoppingCart { Id = 1, ApplicationUserId = userId, ProductId = 1, Count = 1 }
            };

            var memoryCarts = new List<ShoppingCart>
            {
                new ShoppingCart { Id = 2, ProductId = 2, Count = 1 }
            };

            var combinedCarts = new List<ShoppingCart>
            {
                new ShoppingCart { Id = 1, ApplicationUserId = userId, ProductId = 1, Count = 1 },
                new ShoppingCart { Id = 2, ApplicationUserId = userId, ProductId = 2, Count = 1 }
            };

            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 10m },
                new Product { Id = 2, Title = "Product 2", Price = 20m }
            };

            // Setup repository to simulate combining carts
            _mockShoppingCartRepo.Setup(r => r.CombineToDB(dbCarts, memoryCarts, userId))
                .Verifiable();

            // Setup GetShoppingCartsByUserId to return combined carts after combining
            _mockShoppingCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>()))
                .Returns(combinedCarts);

            // Act
            var result = _shoppingCartService.CombineToDB(dbCarts, memoryCarts, userId);

            // Assert
            _mockShoppingCartRepo.Verify(r => r.CombineToDB(dbCarts, memoryCarts, userId), Times.Once);
            _mockMemoryCache.Verify(m => m.Remove(_guestCartKey), Times.Once);
            Assert.NotNull(result);
            Assert.Equal(combinedCarts, result.ShoppingCartList);
        }

        [Fact]
        public void CombineToDB_WithProductPricesAndDiscounts_ShouldCalculateCorrectTotal()
        {
            // Arrange
            string userId = "user123";
            var products = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Price = 10m, DiscountAmount = 2m },
                new Product { Id = 2, Title = "Product 2", Price = 20m, DiscountAmount = 5m }
            };

            var dbCarts = new List<ShoppingCart>
            {
                new ShoppingCart { Id = 1, ApplicationUserId = userId, ProductId = 1, Count = 1, Product = products[0] }
            };

            var memoryCarts = new List<ShoppingCart>
            {
                new ShoppingCart { Id = 2, ProductId = 2, Count = 1, Product = products[1] }
            };

            var combinedCarts = new List<ShoppingCart>
            {
                new ShoppingCart { Id = 1, ApplicationUserId = userId, ProductId = 1, Count = 1, Product = products[0] },
                new ShoppingCart { Id = 2, ApplicationUserId = userId, ProductId = 2, Count = 1, Product = products[1] }
            };

            // Setup repository
            _mockShoppingCartRepo.Setup(r => r.CombineToDB(dbCarts, memoryCarts, userId))
                .Verifiable();

            _mockShoppingCartRepo.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<ShoppingCart, bool>>>(),
                It.IsAny<string>()))
                .Returns(combinedCarts);

            // Act
            var result = _shoppingCartService.CombineToDB(dbCarts, memoryCarts, userId);

            // Assert
            Assert.NotNull(result);
            // (10-2)*1 + (20-5)*1 = 8 + 15 = 23
            Assert.Equal(23.0, result.OrderHeader.OrderTotal);
        }*/
    }
}