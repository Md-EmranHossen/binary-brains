using AmarTech.Application.Services.IServices;
using AmarTech.Domain.Entities;
using AmarTech.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AmarTech.Test.ControllerTests
{
    public class DashboardControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IApplicationUserService> _mockApplicationUserService;
        private readonly Mock<IOrderHeaderService> _mockOrderHeaderService;
        private readonly DashboardController _controller;

        public DashboardControllerTests()
        {
            // Setup mocks for all required services
            _mockProductService = new Mock<IProductService>();
            _mockApplicationUserService = new Mock<IApplicationUserService>();
            _mockOrderHeaderService = new Mock<IOrderHeaderService>();

            // Initialize controller with mocked services
            _controller = new DashboardController(
                _mockProductService.Object,
                _mockApplicationUserService.Object,
                _mockOrderHeaderService.Object
            );
        }

        [Fact]
        public void Index_ReturnsViewWithDashboardVM()
        {
            // Arrange
            const int expectedUserCount = 10;
            const int expectedOrderCount = 25;
            const int expectedProductCount = 50;

            // Setup mock return values
            _mockApplicationUserService.Setup(s => s.GetAllUsersCount()).Returns(expectedUserCount);
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeadersCount()).Returns(expectedOrderCount);
            _mockProductService.Setup(s => s.GetAllProductsCount(It.IsAny<string?>()))
                    .Returns(expectedProductCount);


            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);

            // Verify the model is of correct type
            var model = Assert.IsType<DashboardVM>(result.Model);

            // Verify each property has correct value
            Assert.Equal(expectedUserCount, model.TotalUsers);
            Assert.Equal(expectedOrderCount, model.TotalOrders);
            Assert.Equal(expectedProductCount, model.TotalProducts);

            // Verify each service method was called exactly once
            _mockApplicationUserService.Verify(s => s.GetAllUsersCount(), Times.Once);
            _mockOrderHeaderService.Verify(s => s.GetAllOrderHeadersCount(), Times.Once);
            _mockProductService.Verify(s => s.GetAllProductsCount(It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public void Index_WithZeroCounts_ReturnsViewWithZeroValues()
        {
            // Arrange
            const int zeroCount = 0;

            // Setup mock return values with zeros
            _mockApplicationUserService.Setup(s => s.GetAllUsersCount()).Returns(zeroCount);
            _mockOrderHeaderService.Setup(s => s.GetAllOrderHeadersCount()).Returns(zeroCount);
            _mockProductService.Setup(s => s.GetAllProductsCount(It.IsAny<string?>())).Returns(zeroCount);

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);

            // Verify the model is of correct type
            var model = Assert.IsType<DashboardVM>(result.Model);

            // Verify each property has zero value
            Assert.Equal(zeroCount, model.TotalUsers);
            Assert.Equal(zeroCount, model.TotalOrders);
            Assert.Equal(zeroCount, model.TotalProducts);

            // Verify each service method was called exactly once
            _mockApplicationUserService.Verify(s => s.GetAllUsersCount(), Times.Once);
            _mockOrderHeaderService.Verify(s => s.GetAllOrderHeadersCount(), Times.Once);
            _mockProductService.Verify(s => s.GetAllProductsCount(It.IsAny<string?>()), Times.Once);
        }

        [Fact]
        public void Controller_HasCorrectAreaAttribute()
        {
            // Arrange & Act
            var attributes = typeof(DashboardController).GetCustomAttributes(typeof(AreaAttribute), true);

            // Assert
            Assert.Single(attributes);
            var areaAttribute = attributes[0] as AreaAttribute;
            Assert.NotNull(areaAttribute);
            Assert.Equal("Admin", areaAttribute.RouteValue);
        }

        [Fact]
        public void Controller_HasCorrectAuthorizeAttribute()
        {
            // Arrange & Act
            var attributes = typeof(DashboardController).GetCustomAttributes(typeof(AuthorizeAttribute), true);

            // Assert
            Assert.Single(attributes);
            var authorizeAttribute = attributes[0] as AuthorizeAttribute;
            Assert.NotNull(authorizeAttribute);
            Assert.Equal(SD.Role_Admin, authorizeAttribute.Roles);
        }
    }
}