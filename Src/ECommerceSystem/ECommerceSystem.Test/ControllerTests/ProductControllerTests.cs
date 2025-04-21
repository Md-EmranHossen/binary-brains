using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using ECommerceWebApp.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;

namespace ECommerceSystem.Test.ControllerTests
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IWebHostEnvironment> _mockWebHostEnvironment;
        private readonly ProductController _controller;
        private readonly string _testWebRootPath = "test/wwwroot/path";

        public ProductControllerTests()
        {
            _mockProductService = new Mock<IProductService>();
            _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
            _mockWebHostEnvironment.Setup(x => x.WebRootPath).Returns(_testWebRootPath);

            _controller = new ProductController(_mockProductService.Object, _mockWebHostEnvironment.Object);

            // Setup TempData for the controller
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public void Index_ReturnsViewWithProductList()
        {
            // Arrange
            var expectedProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Test Product 1" },
                new Product { Id = 2, Title = "Test Product 2" }
            };
            _mockProductService.Setup(s => s.GetAllProducts()).Returns(expectedProducts);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Product>>(viewResult.Model);
            Assert.Equal(expectedProducts.Count, model.Count());
            Assert.Equal(expectedProducts, model);
        }

        [Fact]
        public void Create_Get_ReturnsViewWithCategoryList()
        {
            // Arrange
            var expectedCategoryList = new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "Electronics" },
        new SelectListItem { Value = "2", Text = "Accessories" }
    };

            _mockProductService.Setup(s => s.CategoryList()).Returns(expectedCategoryList);

            // Act
             _controller.Create();

            // Assert

            Assert.Equal(expectedCategoryList, _controller.ViewBag.CategoryList);
        }


        [Fact]
        public void Create_Post_InvalidModelState_ReturnsViewWithSameModel()
        {
            // Arrange
            var product = new Product { Title = "Test Product" };
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.Create(product, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(product, model);
        }

        [Fact]
        public void Create_Post_ValidModelState_CreatesProductAndRedirects()
        {
            // Arrange
            var product = new Product { Title = "New Product" };
            var mockFile = new Mock<IFormFile>();

            // Act
            var result = _controller.Create(product, mockFile.Object);

            // Assert
            _mockProductService.Verify(s => s.CreatePathOfProduct(product, mockFile.Object, _testWebRootPath), Times.Once);
            _mockProductService.Verify(s => s.AddProduct(product), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Product created successfully", _controller.TempData["success"]);
        }

        [Fact]
        public void LoadProductViewWithCategories_IdIsNullOrZero_ReturnsNotFound()
        {
            // Call the private method using reflection
            var method = typeof(ProductController).GetMethod("LoadProductViewWithCategories",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Test with null id
            var result1 = method?.Invoke(_controller, new object[] { null!, "Edit" }) as IActionResult;
            Assert.IsType<NotFoundResult>(result1);

            // Test with zero id
            var result2 = method?.Invoke(_controller, new object[] { 0, "Edit" }) as IActionResult;
            Assert.IsType<NotFoundResult>(result2);
        }

        [Fact]
        public void LoadProductViewWithCategories_ProductNotFound_ReturnsNotFound()
        {
            // Arrange
            int productId = 99;
            _mockProductService.Setup(s => s.GetProductById(productId)).Returns((Product?)null);

            // Call the private method using reflection
            var method = typeof(ProductController).GetMethod("LoadProductViewWithCategories",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = method?.Invoke(_controller, new object[] { productId, "Edit" }) as IActionResult;

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void LoadProductViewWithCategories_ProductFound_ReturnsViewWithProductAndCategories()
        {
            // Arrange
            int productId = 1;
            var product = new Product { Id = productId, Title = "Test Product" };

            var categoryList = new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "Category" }
    };

            _mockProductService.Setup(s => s.GetProductById(productId)).Returns(product);
            _mockProductService.Setup(s => s.CategoryList()).Returns(categoryList);

            // Call the private method using reflection
            var method = typeof(ProductController).GetMethod("LoadProductViewWithCategories",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = method?.Invoke(_controller, new object[] { productId, "TestView" }) as IActionResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("TestView", viewResult.ViewName);
            Assert.Equal(product, viewResult.Model);
            Assert.Equal(categoryList, _controller.ViewBag.CategoryList);
        }


        [Fact]
        public void Edit_Get_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.Edit(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Edit_Get_ValidModelState_CallsLoadProductViewWithCategories()
        {
            // Arrange
            int productId = 1;
            var product = new Product
            {
                Id = productId,
                Title = "Test Product",
                CategoryId = 1,
                CreatedBy = "admin"
            };

            var categoryList = new List<SelectListItem>
    {
        new SelectListItem { Value = "1", Text = "Category" }
    };

            _mockProductService.Setup(s => s.GetProductById(productId)).Returns(product);
            _mockProductService.Setup(s => s.CategoryList()).Returns(categoryList);

            // Act
            var result = _controller.Edit(productId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", viewResult.ViewName);
            Assert.Equal(product, viewResult.Model);
            Assert.Equal(categoryList, _controller.ViewBag.CategoryList);
        }


        [Fact]
        public void Edit_Post_InvalidModelState_ReturnsViewWithSameModel()
        {
            // Arrange
            var product = new Product { Id = 1, Title = "Updated Product" };
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.Edit(product, null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Product>(viewResult.Model);
            Assert.Equal(product, model);
        }

        [Fact]
        public void Edit_Post_ValidModelState_UpdatesProductAndRedirects()
        {
            // Arrange
            var product = new Product { Id = 1, Title = "Updated Product" };
            var mockFile = new Mock<IFormFile>();
            DateTime beforeUpdate = DateTime.Now;

            // Act
            var result = _controller.Edit(product, mockFile.Object);

            // Assert
            _mockProductService.Verify(s => s.EditPathOfProduct(product, mockFile.Object, _testWebRootPath), Times.Once);
            _mockProductService.Verify(s => s.UpdateProduct(product), Times.Once);
            Assert.True(product.UpdatedDate >= beforeUpdate);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Product updated successfully", _controller.TempData["success"]);
        }

        [Fact]
        public void Delete_Get_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.Delete(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void Delete_Get_ValidModelState_CallsLoadProductViewWithCategories()
        {
            // Arrange
            int productId = 1;
            var product = new Product
            {
                Id = productId,
                Title = "Test Product",
                CategoryId = 1,
                CreatedBy = "admin"
            };

            var categoryList = new List<SelectListItem>
             {
              new SelectListItem { Value = "1", Text = "Category" }
             };

            _mockProductService.Setup(s => s.GetProductById(productId)).Returns(product);
            _mockProductService.Setup(s => s.CategoryList()).Returns(categoryList);

            // Act
            var result = _controller.Delete(productId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Delete", viewResult.ViewName);
            Assert.Equal(product, viewResult.Model);
            Assert.Equal(categoryList, _controller.ViewBag.CategoryList);
        }


        [Fact]
        public void DeletePost_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.DeletePost(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void DeletePost_IdIsNull_ReturnsNotFound()
        {
            // Act
            var result = _controller.DeletePost(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeletePost_ValidIdProvided_DeletesProductAndRedirects()
        {
            // Arrange
            int productId = 1;

            // Act
            var result = _controller.DeletePost(productId);

            // Assert
            _mockProductService.Verify(s => s.DeleteProduct(productId), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Product deleted successfully", _controller.TempData["success"]);
        }
    }
}