using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using ECommerceWebApp.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace ECommerceSystem.Test.ControllerTests
{
    public class CategoryControllerTests
    {
        private readonly Mock<ICategoryService> _mockCategoryService;
        private readonly CategoryController _controller;
        private readonly List<Category> _categories;

        public CategoryControllerTests()
        {
            _mockCategoryService = new Mock<ICategoryService>();
            _controller = new CategoryController(_mockCategoryService.Object);

            // Setup TempData for controller
            var httpContext = new DefaultHttpContext();
            var tempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
            _controller.TempData = tempData;

            // Sample data
            _categories = new List<Category>
            {
                new Category { Id = 1, Name = "Electronics", DsiplayOrder = 1,CreatedBy = "admin" },
                new Category { Id = 2, Name = "Books", DsiplayOrder = 2,CreatedBy = "admin" },
                new Category { Id = 3, Name = "Clothing", DsiplayOrder = 3 , CreatedBy = "admin"}
            };
        }

        [Fact]
        public void Index_ReturnsViewWithAllCategories()
        {
            // Arrange
            _mockCategoryService.Setup(service => service.GetAllCategories())
                .Returns(_categories);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Category>>(viewResult.Model);
            Assert.Equal(3, ((List<Category>)model).Count);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            // Act
            var result = _controller.Create();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Create_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var newCategory = new Category { Name = "Test Category", DsiplayOrder = 4 ,CreatedBy= "admin" };
            _mockCategoryService.Setup(service => service.AddCategory(It.IsAny<Category>()));

            // Act
            var result = _controller.Create(newCategory);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Category created successfully", _controller.TempData["success"]);
            _mockCategoryService.Verify(s => s.AddCategory(It.IsAny<Category>()), Times.Once);
        }

        [Fact]
        public void Create_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var invalidCategory = new Category() { Name = "Test Category", DsiplayOrder = 4, CreatedBy = "admin" };
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = _controller.Create(invalidCategory);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(invalidCategory, viewResult.Model);
            _mockCategoryService.Verify(s => s.AddCategory(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public void Edit_Get_WithValidId_ReturnsViewWithCategory()
        {
            // Arrange
            int categoryId = 1;
            var category = _categories[0];
            _mockCategoryService.Setup(service => service.GetCategoryById(categoryId))
                .Returns(category);

            // Act
            var result = _controller.Edit(categoryId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Category>(viewResult.Model);
            Assert.Equal(categoryId, model.Id);
        }

        [Fact]
        public void Edit_Get_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int? categoryId = null;

            // Act
            var result = _controller.Edit(categoryId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Get_WithNonExistentId_ReturnsNotFound()
        {
            // Arrange
            int categoryId = 999;
            _mockCategoryService.Setup(service => service.GetCategoryById(categoryId))
                .Returns((Category?)null);

            // Act
            var result = _controller.Edit(categoryId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Edit_Post_ValidModel_RedirectsToIndex()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Updated Category", CreatedBy = "admin" };
            _mockCategoryService.Setup(service => service.UpdateCategory(It.IsAny<Category>()));

            // Act
            var result = _controller.Edit(category);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Category updated successfully", _controller.TempData["success"]);
            _mockCategoryService.Verify(s => s.UpdateCategory(It.Is<Category>(c => c.UpdatedDate != DateTime.MinValue)), Times.Once);
        }

        [Fact]
        public void Edit_Post_InvalidModel_ReturnsViewWithModel()
        {
            // Arrange
            var invalidCategory = new Category { Id = 1, Name = "Updated Category", CreatedBy = "admin" };
            _controller.ModelState.AddModelError("Name", "Name is required");

            // Act
            var result = _controller.Edit(invalidCategory);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(invalidCategory, viewResult.Model);
            _mockCategoryService.Verify(s => s.UpdateCategory(It.IsAny<Category>()), Times.Never);
        }

        [Fact]
        public void Delete_Get_WithValidId_ReturnsViewWithCategory()
        {
            // Arrange
            int categoryId = 1;
            var category = _categories[0];
            _mockCategoryService.Setup(service => service.GetCategoryById(categoryId))
                .Returns(category);

            // Act
            var result = _controller.Delete(categoryId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Category>(viewResult.Model);
            Assert.Equal(categoryId, model.Id);
        }

        [Fact]
        public void Delete_Get_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            int? categoryId = null;

            // Act
            var result = _controller.Delete(categoryId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteConfirmed_WithValidId_RedirectsToIndex()
        {
            // Arrange
            int categoryId = 1;
            _mockCategoryService.Setup(service => service.DeleteCategory(It.IsAny<int?>()));

            // Act
            var result = _controller.DeleteConfirmed(categoryId);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Category deleted successfully", _controller.TempData["success"]);
            _mockCategoryService.Verify(s => s.DeleteCategory(categoryId), Times.Once);
        }

        [Fact]
        public void DeleteConfirmed_WithNullId_ReturnsNotFound()
        {
            // Arrange
            int? categoryId = null;

            // Act
            var result = _controller.DeleteConfirmed(categoryId);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockCategoryService.Verify(s => s.DeleteCategory(It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public void DeleteConfirmed_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            int categoryId = 1;
            _controller.ModelState.AddModelError("Key", "Error message");

            // Act
            var result = _controller.DeleteConfirmed(categoryId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mockCategoryService.Verify(s => s.DeleteCategory(It.IsAny<int?>()), Times.Never);
        }
        [Fact]
        public void Delete_Get_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            int categoryId = 1;
            _controller.ModelState.AddModelError("Key", "Error message");

            // Act
            var result = _controller.Delete(categoryId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mockCategoryService.Verify(s => s.GetCategoryById(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void EditGet_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            int companyId = 1;
            _controller.ModelState.AddModelError("Key", "Error message");

            // Act
            var result = _controller.Edit(companyId);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _mockCategoryService.Verify(s => s.GetCategoryById(It.IsAny<int>()), Times.Never);
        }
    }
}