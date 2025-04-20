using ECommerceSystem.Models;
using ECommerceWebApp.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace ECommerceSystem.Test.ControllerTests
{
    public class DashboardControllerTests
    {
        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Arrange
            var controller = new DashboardController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Controller_HasAreaAttribute()
        {
            // Arrange
            var controllerType = typeof(DashboardController);

            // Act
            var areaAttribute = controllerType.GetCustomAttribute<AreaAttribute>();

            // Assert
            Assert.NotNull(areaAttribute);
            Assert.Equal("Admin", areaAttribute.RouteValue);
        }

        [Fact]
        public void Controller_HasAuthorizeAttribute()
        {
            // Arrange
            var controllerType = typeof(DashboardController);

            // Act
            var authorizeAttribute = controllerType.GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.NotNull(authorizeAttribute);
            Assert.Equal(SD.Role_Admin, authorizeAttribute.Roles);
        }
    }
}