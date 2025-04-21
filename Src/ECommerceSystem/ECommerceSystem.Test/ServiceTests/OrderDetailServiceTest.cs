using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace ECommerceSystem.Test.ServiceTests
{
    public class OrderDetailServiceTests
    {
        private readonly Mock<IOrderDetailRepository> _mockOrderDetailRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly OrderDetailService _service;

        public OrderDetailServiceTests()
        {
            _mockOrderDetailRepository = new Mock<IOrderDetailRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _service = new OrderDetailService(_mockOrderDetailRepository.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public void GetAllOrderDetails_ReturnsAllOrderDetailsFromRepository()
        {
            // Arrange
            var expectedOrderDetails = new List<OrderDetail>
            {
                new OrderDetail { Id = 1, OrderHeaderId = 1, ProductId = 1, Price = 100.4, Count = 2 },
                new OrderDetail { Id = 2, OrderHeaderId = 1, ProductId = 2, Price = 50.0, Count = 1 }
            };

            _mockOrderDetailRepository.Setup(r => r.GetAll(null, null))
                .Returns(expectedOrderDetails);

            // Act
            var result = _service.GetAllOrderDetails();

            // Assert
            Assert.Equal(expectedOrderDetails, result);
            _mockOrderDetailRepository.Verify(r => r.GetAll(null, null), Times.Once);
        }

        [Fact]
        public void GetAllOrders_WithValidOrderHeaderId_ReturnsFilteredOrderDetails()
        {
            // Arrange
            int orderHeaderId = 1;
            var expectedOrderDetails = new List<OrderDetail>
            {
                new OrderDetail { Id = 1, OrderHeaderId = orderHeaderId, ProductId = 1, Price = 100.4, Count = 2 },
                new OrderDetail { Id = 2, OrderHeaderId = orderHeaderId, ProductId = 2, Price = 50.0, Count = 1 }
            };

            _mockOrderDetailRepository.Setup(r => r.GetAll(It.Is<Expression<Func<OrderDetail, bool>>>(expr => expr.Compile()(expectedOrderDetails[0])), "Product"))
                .Returns(expectedOrderDetails);

            // Act
            var result = _service.GetAllOrders(orderHeaderId, "Product");

            // Assert
            Assert.Equal(expectedOrderDetails, result);
            _mockOrderDetailRepository.Verify(r => r.GetAll(It.IsAny<Expression<Func<OrderDetail, bool>>>(), "Product"), Times.Once);
        }

        [Fact]
        public void GetAllOrders_WithNullOrderHeaderId_ReturnsEmptyList()
        {
            // Arrange
            int? orderHeaderId = null;

            _mockOrderDetailRepository.Setup(r => r.GetAll(It.IsAny<Expression<Func<OrderDetail, bool>>>(), null))
                .Returns(new List<OrderDetail>());

            // Act
            var result = _service.GetAllOrders(orderHeaderId);

            // Assert
            Assert.Empty(result);
            _mockOrderDetailRepository.Verify(r => r.GetAll(It.IsAny<Expression<Func<OrderDetail, bool>>>(), null), Times.Once);
        }

        [Fact]
        public void GetOrderDetailById_WithValidId_ReturnsOrderDetail()
        {
            // Arrange
            int orderDetailId = 1;
            var expectedOrderDetail = new OrderDetail { Id = orderDetailId, OrderHeaderId = 1, ProductId = 1, Price = 100.4, Count = 2 };

            _mockOrderDetailRepository.Setup(r => r.Get(It.Is<Expression<Func<OrderDetail, bool>>>(expr => expr.Compile()(expectedOrderDetail)), null, false))
                .Returns(expectedOrderDetail);

            // Act
            var result = _service.GetOrderDetailById(orderDetailId);

            // Assert
            Assert.Equal(expectedOrderDetail, result);
            _mockOrderDetailRepository.Verify(r => r.Get(It.IsAny<Expression<Func<OrderDetail, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void GetOrderDetailById_WithNullId_CallsRepositoryAndReturnsNull()
        {
            // Arrange
            int? orderDetailId = null;

            _mockOrderDetailRepository.Setup(r => r.Get(It.IsAny<Expression<Func<OrderDetail, bool>>>(), null, false))
                .Returns((OrderDetail?)null);

            // Act
            var result = _service.GetOrderDetailById(orderDetailId);

            // Assert
            Assert.Null(result);
            _mockOrderDetailRepository.Verify(r => r.Get(It.IsAny<Expression<Func<OrderDetail, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void GetOrderDetailById_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            int orderDetailId = 999;

            _mockOrderDetailRepository.Setup(r => r.Get(It.IsAny<Expression<Func<OrderDetail, bool>>>(), null, false))
                .Returns((OrderDetail?)null);

            // Act
            var result = _service.GetOrderDetailById(orderDetailId);

            // Assert
            Assert.Null(result);
            _mockOrderDetailRepository.Verify(r => r.Get(It.IsAny<Expression<Func<OrderDetail, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void AddOrderDetail_AddsToRepositoryAndCommits()
        {
            // Arrange
            var orderDetail = new OrderDetail { Id = 1, OrderHeaderId = 1, ProductId = 1, Price = 100.4, Count = 2 };

            _mockOrderDetailRepository.Setup(r => r.Add(orderDetail));
            _mockUnitOfWork.Setup(u => u.Commit());

            // Act
            _service.AddOrderDetail(orderDetail);

            // Assert
            _mockOrderDetailRepository.Verify(r => r.Add(orderDetail), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateOrderDetail_UpdatesRepositoryAndCommits()
        {
            // Arrange
            var orderDetail = new OrderDetail { Id = 1, OrderHeaderId = 1, ProductId = 1, Price = 150.0, Count = 3 };

            _mockOrderDetailRepository.Setup(r => r.Update(orderDetail));
            _mockUnitOfWork.Setup(u => u.Commit());

            // Act
            _service.UpdateOrderDetail(orderDetail);

            // Assert
            _mockOrderDetailRepository.Verify(r => r.Update(orderDetail), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void DeleteOrderDetail_WithNullId_DoesNotCallRepository()
        {
            // Arrange
            int? orderDetailId = null;

            // Act
            _service.DeleteOrderDetail(orderDetailId);

            // Assert
            _mockOrderDetailRepository.Verify(r => r.Get(It.IsAny<Expression<Func<OrderDetail, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
            _mockOrderDetailRepository.Verify(r => r.Remove(It.IsAny<OrderDetail>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteOrderDetail_WithValidIdButOrderDetailNotFound_DoesNotRemove()
        {
            // Arrange
            int orderDetailId = 1;

            _mockOrderDetailRepository.Setup(r => r.Get(It.IsAny<Expression<Func<OrderDetail, bool>>>(), null, false))
                .Returns((OrderDetail?)null);

            // Act
            _service.DeleteOrderDetail(orderDetailId);

            // Assert
            _mockOrderDetailRepository.Verify(r => r.Get(It.IsAny<Expression<Func<OrderDetail, bool>>>(), null, false), Times.Once);
            _mockOrderDetailRepository.Verify(r => r.Remove(It.IsAny<OrderDetail>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteOrderDetail_WithValidIdAndOrderDetailFound_RemovesAndCommits()
        {
            // Arrange
            int orderDetailId = 1;
            var orderDetail = new OrderDetail { Id = orderDetailId, OrderHeaderId = 1, ProductId = 1, Price = 100.4, Count = 2 };

            _mockOrderDetailRepository.Setup(r => r.Get(It.IsAny<Expression<Func<OrderDetail, bool>>>(), null, false))
                .Returns(orderDetail);
            _mockOrderDetailRepository.Setup(r => r.Remove(orderDetail));
            _mockUnitOfWork.Setup(u => u.Commit());

            // Act
            _service.DeleteOrderDetail(orderDetailId);

            // Assert
            _mockOrderDetailRepository.Verify(r => r.Get(It.IsAny<Expression<Func<OrderDetail, bool>>>(), null, false), Times.Once);
            _mockOrderDetailRepository.Verify(r => r.Remove(orderDetail), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateOrderDetailsValues_WithValidShoppingCart_AddsOrderDetails()
        {
            // Arrange
            var orderHeader = new OrderHeader { Id = 1 };
            var shoppingCartList = new List<ShoppingCart>
            {
                new ShoppingCart
                {
                    ProductId = 1,
                    Count = 2,
                    Product = new Product { Price = 100.4M }
                },
                new ShoppingCart
                {
                    ProductId = 2,
                    Count = 1,
                    Product = new Product { Price = 50.0M }
                }
            };
            var shoppingCartVM = new ShoppingCartVM
            {
                OrderHeader = orderHeader,
                ShoppingCartList = shoppingCartList
            };

            _mockOrderDetailRepository.Setup(r => r.Add(It.IsAny<OrderDetail>()));
            _mockUnitOfWork.Setup(u => u.Commit());

            // Act
            _service.UpdateOrderDetailsValues(shoppingCartVM);

            // Assert
            _mockOrderDetailRepository.Verify(r => r.Add(It.Is<OrderDetail>(od =>
                od.ProductId == 1 && od.OrderHeaderId == 1 && od.Price == 100.4 && od.Count == 2)), Times.Once); // Fixed Price to 100.4M
            _mockOrderDetailRepository.Verify(r => r.Add(It.Is<OrderDetail>(od =>
                od.ProductId == 2 && od.OrderHeaderId == 1 && od.Price == 50.0 && od.Count == 1)), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Exactly(3)); // Once per AddOrderDetail call + final Commit
        }

        [Fact]
        public void UpdateOrderDetailsValues_WithNullProductInCart_SkipsInvalidItems()
        {
            // Arrange
            var orderHeader = new OrderHeader { Id = 1 };
            var shoppingCartList = new List<ShoppingCart>
            {
                new ShoppingCart
                {
                    ProductId = 1,
                    Count = 2,
                    Product = null! // Invalid item
                },
                new ShoppingCart
                {
                    ProductId = 2,
                    Count = 1,
                    Product = new Product { Price = 50.0M }
                }
            };
            var shoppingCartVM = new ShoppingCartVM
            {
                OrderHeader = orderHeader,
                ShoppingCartList = shoppingCartList
            };

            _mockOrderDetailRepository.Setup(r => r.Add(It.IsAny<OrderDetail>()));
            _mockUnitOfWork.Setup(u => u.Commit());

            // Act
            _service.UpdateOrderDetailsValues(shoppingCartVM);

            // Assert
            _mockOrderDetailRepository.Verify(r => r.Add(It.IsAny<OrderDetail>()), Times.Once); // Only one valid item
            _mockOrderDetailRepository.Verify(r => r.Add(It.Is<OrderDetail>(od =>
                od.ProductId == 2 && od.OrderHeaderId == 1 && od.Price == 50.0 && od.Count == 1)), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Exactly(2)); // Once for the valid AddOrderDetail + final Commit
        }

        [Fact]
        public void UpdateOrderDetailsValues_WithEmptyShoppingCart_CommitsOnce()
        {
            // Arrange
            var orderHeader = new OrderHeader { Id = 1 };
            var shoppingCartVM = new ShoppingCartVM
            {
                OrderHeader = orderHeader,

            };

            _mockUnitOfWork.Setup(u => u.Commit());

            // Act
            _service.UpdateOrderDetailsValues(shoppingCartVM);

            // Assert
            _mockOrderDetailRepository.Verify(r => r.Add(It.IsAny<OrderDetail>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once); // Only the final Commit
        }
    }
}