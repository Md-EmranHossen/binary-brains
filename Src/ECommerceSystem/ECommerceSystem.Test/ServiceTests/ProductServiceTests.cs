using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceWebApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Xunit;

namespace ECommerceSystem.Test.ServiceTests
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly ProductService _productService;

        public ProductServiceTests()
        {
            _mockProductRepository = new Mock<IProductRepository>();
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();

            _productService = new ProductService(
                _mockProductRepository.Object,
                _mockCategoryRepository.Object,
                _mockUnitOfWork.Object
            );
        }

        [Fact]
        public void GetAllProducts_ShouldReturnAllProducts()
        {
            // Arrange
            var expectedProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Category = new Category { Id = 1, Name = "Category 1" ,CreatedBy="admin"} },
                new Product { Id = 2, Title = "Product 2", Category = new Category { Id = 2, Name = "Category 2",CreatedBy = "admin" } }
            };

            _mockProductRepository.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.IsAny<string>()))
                .Returns(expectedProducts);

            // Act
            var result = _productService.GetAllProducts();

            // Assert
            Assert.Equal(expectedProducts, result);
            _mockProductRepository.Verify(r => r.GetAll(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.Is<string>(s => s == "Category")), Times.Once);
        }

        [Fact]
        public void GetProductById_WithValidId_ShouldReturnProduct()
        {
            // Arrange
            var expectedProduct = new Product { Id = 1, Title = "Product 1" };
            _mockProductRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>(), false))
                .Returns(expectedProduct);

            // Act
            var result = _productService.GetProductById(1);

            // Assert
            Assert.Equal(expectedProduct, result);
            _mockProductRepository.Verify(r => r.Get(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>(), false), Times.Once);
        }

        [Fact]
        public void GetProductById_WithNullId_ShouldReturnNull()
        {
            // Act
            var result = _productService.GetProductById(null);

            // Assert
            Assert.Null(result);
            _mockProductRepository.Verify(r => r.Get(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>(), false), Times.Never);
        }

        [Fact]
        public void AddProduct_WithValidProduct_ShouldAddProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Title = "Product 1" };

            // Act
            _productService.AddProduct(product);

            // Assert
            _mockProductRepository.Verify(r => r.Add(product), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void AddProduct_WithNullProduct_ShouldNotAddProduct()
        {
            // Act
            _productService.AddProduct(null);

            // Assert
            _mockProductRepository.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void UpdateProduct_WithValidProduct_ShouldUpdateProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Title = "Updated Product" };

            // Act
            _productService.UpdateProduct(product);

            // Assert
            _mockProductRepository.Verify(r => r.Update(product), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateProduct_WithNullProduct_ShouldNotUpdateProduct()
        {
            // Act
            _productService.UpdateProduct(null);

            // Assert
            _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteProduct_WithValidId_ShouldDeleteProduct()
        {
            // Arrange
            var product = new Product { Id = 1, Title = "Product 1" };
            _mockProductRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>(), false))
                .Returns(product);

            // Act
            _productService.DeleteProduct(1);

            // Assert
            _mockProductRepository.Verify(r => r.Remove(product), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void DeleteProduct_WithNullId_ShouldNotDeleteProduct()
        {
            // Act
            _productService.DeleteProduct(null);

            // Assert
            _mockProductRepository.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteProduct_WithNonExistentId_ShouldNotDeleteProduct()
        {
            // Arrange
            _mockProductRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<string>(),false))
                .Returns((Product)null);

            // Act
            _productService.DeleteProduct(999);

            // Assert
            _mockProductRepository.Verify(r => r.Remove(It.IsAny<Product>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void CategoryList_ShouldReturnCategorySelectList()
        {
            // Arrange
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Category 1",CreatedBy = "admin" },
                new Category { Id = 2, Name = "Category 2" ,CreatedBy = "admin"}
            };

            _mockCategoryRepository.Setup(r => r.GetAll(
                It.IsAny<Expression<Func<Category, bool>>>(),
                It.IsAny<string>()))
                .Returns(categories);


            // Act
            var result = _productService.CategoryList().ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Category 1", result[0].Text);
            Assert.Equal("1", result[0].Value);
            Assert.Equal("Category 2", result[1].Text);
            Assert.Equal("2", result[1].Value);
        }

        [Fact]
        public void EditPathOfProduct_WithValidInputs_ShouldUpdateImageUrl()
        {
            // Arrange
            var product = new Product { Id = 1, Title = "Product 1", ImageUrl = "/images/product/oldimage.jpg" };
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.jpg");

            var wwwRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(wwwRootPath);

            try
            {
                // Create the old image to test deletion
                var imageFolder = Path.Combine(wwwRootPath, "images", "product");
                Directory.CreateDirectory(imageFolder);
                var oldImagePath = Path.Combine(imageFolder, "oldimage.jpg");
                File.Create(oldImagePath).Dispose();

                mockFile.Setup(f => f.CopyTo(It.IsAny<Stream>()))
                    .Callback<Stream>(stream => {
                        var writer = new StreamWriter(stream);
                        writer.WriteLine("test content");
                        writer.Flush();
                    });

                // Act
                _productService.EditPathOfProduct(product, mockFile.Object, wwwRootPath);

                // Assert
                Assert.StartsWith("/images/product/", product.ImageUrl);
                Assert.True(product.ImageUrl.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase));
                Assert.DoesNotContain("oldimage.jpg", product.ImageUrl); // Ensure it's updated
                Assert.NotEqual("/images/product/oldimage.jpg", product.ImageUrl); // Ensure it's not the same
            }
            finally
            {
                if (Directory.Exists(wwwRootPath))
                {
                    Directory.Delete(wwwRootPath, true);
                }
            }
        }


        [Fact]
        public void EditPathOfProduct_WithNullProduct_ShouldNotUpdateImageUrl()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var wwwRootPath = "C:\\fakepath";

            // Act
            _productService.EditPathOfProduct(null, mockFile.Object, wwwRootPath);

            // Assert - no exception and no change
            mockFile.Verify(f => f.CopyTo(It.IsAny<Stream>()), Times.Never);
        }

        [Fact]
        public void EditPathOfProduct_WithNullFile_ShouldNotUpdateImageUrl()
        {
            // Arrange
            var product = new Product { Id = 1, Title = "Product 1", ImageUrl = "/images/product/oldimage.jpg" };
            var oldImageUrl = product.ImageUrl;
            var wwwRootPath = "C:\\fakepath";

            // Act
            _productService.EditPathOfProduct(product, null, wwwRootPath);

            // Assert
            Assert.Equal(oldImageUrl, product.ImageUrl);
        }

        [Fact]
        public void CreatePathOfProduct_WithValidInputs_ShouldSetImageUrl()
        {
            // Arrange
            var product = new Product { Id = 1, Title = "Product 1" };
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.FileName).Returns("test.jpg");

            var wwwRootPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(wwwRootPath);

            try
            {
                mockFile.Setup(f => f.CopyTo(It.IsAny<Stream>()))
                    .Callback<Stream>(stream => {
                        var writer = new StreamWriter(stream);
                        writer.WriteLine("test content");
                        writer.Flush();
                    });

                // Act
                _productService.CreatePathOfProduct(product, mockFile.Object, wwwRootPath);

                // Assert
                Assert.StartsWith("/images/product/", product.ImageUrl);
                Assert.True(product?.ImageUrl?.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase));

                // Use regex to check if the URL contains a valid GUID pattern
                var guidPattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
                var match = Regex.Match(product?.ImageUrl??"", guidPattern);
                Assert.True(match.Success, "The ImageUrl does not contain a valid GUID.");
            }
            finally
            {
                // Cleanup
                if (Directory.Exists(wwwRootPath))
                {
                    Directory.Delete(wwwRootPath, true);
                }
            }
        }






        [Fact]
        public void CreatePathOfProduct_WithNullProduct_ShouldNotSetImageUrl()
        {
            // Arrange
            var mockFile = new Mock<IFormFile>();
            var wwwRootPath = "C:\\fakepath";

            // Act
            _productService.CreatePathOfProduct(null, mockFile.Object, wwwRootPath);

            // Assert - no exception and no change
            mockFile.Verify(f => f.CopyTo(It.IsAny<Stream>()), Times.Never);
        }

        [Fact]
        public void GetProductByIdwithCategory_ShouldReturnProductWithCategory()
        {
            // Arrange
            var expectedProduct = new Product
            {
                Id = 1,
                Title = "Product 1",
                Category = new Category { Id = 1, Name = "Category 1" ,CreatedBy="admin"}
            };

            _mockProductRepository.Setup(r => r.Get(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.Is<string>(s => s == "Category"),false))
                .Returns(expectedProduct);

            // Act
            var result = _productService.GetProductByIdwithCategory(1);

            // Assert
            Assert.Equal(expectedProduct, result);
            Assert.NotNull(result.Category);
            Assert.Equal("Category 1", result.Category.Name);
            _mockProductRepository.Verify(r => r.Get(
                It.IsAny<Expression<Func<Product, bool>>>(),
                It.Is<string>(s => s == "Category"), false), Times.Once);
        }
    }
}