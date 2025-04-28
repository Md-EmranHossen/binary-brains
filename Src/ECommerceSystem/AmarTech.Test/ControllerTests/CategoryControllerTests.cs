using AmarTech.Application.Services.IServices;
using AmarTech.Domain.Entities;
using AmarTech.Web.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using Xunit;

namespace AmarTech.Test.ControllerTests
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly Mock<IApplicationUserService> _mockApplicationUserService;
        private readonly CategoryController _controller;
        private readonly string _testUserId = "test-user-id";
        private readonly string _testUserName = "test-user";

        public CategoryControllerTests()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _mockApplicationUserService = new Mock<IApplicationUserService>();

            _controller = new CategoryController(
                _mockCategoryService.Object,
                _mockApplicationUserService.Object
            );

            // Set up TempData
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>()
            );

            // Set up user identity by default
            SetupUserIdentity();
        }

        private void SetupUserIdentity()
        {
            // Create claims identity
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, _testUserId)
            };
            var identity = new ClaimsIdentity(claims);
            var principal = new ClaimsPrincipal(identity);

            // Mock HttpContext
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(m => m.User).Returns(principal);

            // Set up ControllerContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object,
                RouteData = new RouteData()
            };

            // Set up user name lookup
            _mockApplicationUserService.Setup(s => s.GetUserName(_testUserId)).Returns(_testUserName);
        }

        [Fact]
        public void Index_ReturnsViewWithCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Test Category" }
            };
            _mockCategoryService.Setup(s => s.GetAllCategories()).Returns(categories);

            // Act
            var result = _controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(categories, result.Model);
            _mockCategoryService.Verify(s => s.GetAllCategories(), Times.Once);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            // Act
            var result = _controller.Create() as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.ViewName); // Default view
            Assert.Null(result.Model); // No model passed
        }

        [Fact]
        public void Create_Post_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var category = new Category { Name = "Test Category" };
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = _controller.Create(category) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category, result.Model);
            _mockCategoryService.Verify(s => s.AddCategory(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public void Create_Post_ValidModelState_AddsCategory()
        {
            // Arrange
            var category = new Category { Name = "Test Category" };

            // Act
            var result = _controller.Create(category) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal(_testUserName, category.CreatedBy);
            _mockCategoryService.Verify(s => s.AddCategory(category), Times.Once);
            Assert.Equal("Category created successfully", _controller.TempData["success"]);
        }

        [Fact]
        public void LoadCategoryView_NullId_ReturnsNotFound()
        {
            // Arrange - Setup private method access via reflection
            var method = typeof(CategoryController).GetMethod("LoadCategoryView",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = method?.Invoke(_controller, new object[] { null, "Edit" }) as IActionResult;

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void LoadCategoryView_ZeroId_ReturnsNotFound()
        {
            // Arrange - Setup private method access via reflection
            var method = typeof(CategoryController).GetMethod("LoadCategoryView",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = method?.Invoke(_controller, new object[] { 0, "Edit" }) as IActionResult;

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void LoadCategoryView_CategoryNotFound_ReturnsNotFound()
        {
            // Arrange
            int categoryId = 1;
            _mockCategoryService.Setup(s => s.GetCategoryById(categoryId)).Returns((Category?)null);

            // Setup private method access via reflection
            var method = typeof(CategoryController).GetMethod("LoadCategoryView",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = method?.Invoke(_controller, new object[] { categoryId, "Edit" }) as IActionResult;

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockCategoryService.Verify(s => s.GetCategoryById(categoryId), Times.Once);
        }

        [Fact]
        public void LoadCategoryView_CategoryFound_ReturnsView()
        {
            // Arrange
            int categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Test Category" };
            _mockCategoryService.Setup(s => s.GetCategoryById(categoryId)).Returns(category);

            // Setup private method access via reflection
            var method = typeof(CategoryController).GetMethod("LoadCategoryView",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = method?.Invoke(_controller, new object[] { categoryId, "Edit" }) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Edit", result.ViewName);
            Assert.Equal(category, result.Model);
            _mockCategoryService.Verify(s => s.GetCategoryById(categoryId), Times.Once);
        }

        [Fact]
        public void Edit_Get_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = _controller.Edit(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Edit_Get_ValidModelState_CallsLoadCategoryView()
        {
            // Arrange
            int categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Test Category" };
            _mockCategoryService.Setup(s => s.GetCategoryById(categoryId)).Returns(category);

            // Act
            var result = _controller.Edit(categoryId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category, result.Model);
            _mockCategoryService.Verify(s => s.GetCategoryById(categoryId), Times.Once);
        }

        [Fact]
        public void Edit_Post_InvalidModelState_ReturnsViewWithModel()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = _controller.Edit(category) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category, result.Model);
            _mockCategoryService.Verify(s => s.UpdateCategory(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public void Edit_Post_ValidModelState_UpdatesCategory()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Test Category" };
            var initialDate = DateTime.Now.AddDays(-1);
            category.UpdatedDate = initialDate;

            // Act
            var result = _controller.Edit(category) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            Assert.Equal(_testUserName, category.UpdatedBy);
            Assert.True(category.UpdatedDate > initialDate); // Verify date was updated
            _mockCategoryService.Verify(s => s.UpdateCategory(category), Times.Once);
            Assert.Equal("Category updated successfully", _controller.TempData["success"]);
        }

        [Fact]
        public void Delete_Get_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = _controller.Delete(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Delete_Get_ValidModelState_CallsLoadCategoryView()
        {
            // Arrange
            int categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Test Category" };
            _mockCategoryService.Setup(s => s.GetCategoryById(categoryId)).Returns(category);

            // Act
            var result = _controller.Delete(categoryId) as ViewResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(category, result.Model);
            _mockCategoryService.Verify(s => s.GetCategoryById(categoryId), Times.Once);
        }

        [Fact]
        public void DeleteConfirmed_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("error", "test error");

            // Act
            var result = _controller.DeleteConfirmed(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void DeleteConfirmed_NullId_ReturnsNotFound()
        {
            // Act
            var result = _controller.DeleteConfirmed(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteConfirmed_ValidId_DeletesCategory()
        {
            // Arrange
            int categoryId = 1;

            // Act
            var result = _controller.DeleteConfirmed(categoryId) as RedirectToActionResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Index", result.ActionName);
            _mockCategoryService.Verify(s => s.DeleteCategory(categoryId), Times.Once);
            Assert.Equal("Category deleted successfully", _controller.TempData["success"]);
        }

        [Fact]
        public void GetCurrentUserName_ReturnsUserName()
        {
            // Act
            var result = _controller.GetCurrentUserName();

            // Assert
            Assert.Equal(_testUserName, result);
            _mockApplicationUserService.Verify(s => s.GetUserName(_testUserId), Times.Once);
        }

        [Fact]
        public void GetCurrentUserName_NoIdentity_ReturnsNull()
        {
            // Arrange
            var mockIdentity = new Mock<IIdentity>();
            mockIdentity.Setup(i => i.IsAuthenticated).Returns(false);

            var mockPrincipal = new Mock<ClaimsPrincipal>();
            mockPrincipal.Setup(p => p.Identity).Returns(mockIdentity.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = mockPrincipal.Object }
            };

            // Act
            var result = _controller.GetCurrentUserName();

            // Assert
            Assert.Null(result);
        }
    }
}