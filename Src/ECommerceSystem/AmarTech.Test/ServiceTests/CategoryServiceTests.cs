using AmarTech.Domain.Entities;
using AmarTech.Infrastructure.Repository.IRepository;
using AmarTech.Application.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AmarTech.Test.ServiceTests
{
    public class CategoryServiceTests
    {
        private readonly Mock<ICategoryRepository> _mockCategoryRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _mockCategoryRepository = new Mock<ICategoryRepository>();
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _service = new CategoryService(_mockCategoryRepository.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public void GetAllCategories_ReturnsAllCategoriesFromRepository()
        {
            // Arrange
            var expectedCategories = new List<Category>
            {
                new Category { Id = 1, Name = "Electronics", DsiplayOrder = 1, CreatedBy = "Rifatul" },
                new Category { Id = 2, Name = "Clothing", DsiplayOrder = 2, CreatedBy = "Rifatul" },
                new Category { Id = 3, Name = "Books", DsiplayOrder = 3, CreatedBy = "Rifatul" }
            };

            _mockCategoryRepository.Setup(r => r.GetAll(null, null))
                .Returns(expectedCategories);

            // Act
            var result = _service.GetAllCategories();

            // Assert
            Assert.Equal(expectedCategories, result);
            _mockCategoryRepository.Verify(r => r.GetAll(null, null), Times.Once);
        }

        [Fact]
        public void GetCategoryById_WithValidId_ReturnsCategoryFromRepository()
        {
            // Arrange
            int categoryId = 1;
            var expectedCategory = new Category { Id = categoryId, Name = "Electronics", CreatedBy = "Rifatul" };

            _mockCategoryRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false))
                .Returns(expectedCategory);

            // Act
            var result = _service.GetCategoryById(categoryId);

            // Assert
            Assert.Equal(expectedCategory, result);
            _mockCategoryRepository.Verify(r => r.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void GetCategoryById_WithNullId_CallsRepositoryWithNullId()
        {
            // Arrange
            int? categoryId = null;

            _mockCategoryRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false))
                .Returns((Category?)null);

            // Act
            var result = _service.GetCategoryById(categoryId);

            // Assert
            Assert.Null(result);
            _mockCategoryRepository.Verify(r => r.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void AddCategory_AddsToRepositoryAndCommits()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Electronics", CreatedBy = "Rifatul" };

            _mockCategoryRepository.Setup(r => r.Add(category));
            _mockUnitOfWork.Setup(u => u.Commit());

            // Act
            _service.AddCategory(category);

            // Assert
            _mockCategoryRepository.Verify(r => r.Add(category), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateCategory_UpdatesRepositoryAndCommits()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Updated Electronics", CreatedBy = "Rifatul" };

            _mockCategoryRepository.Setup(r => r.Update(category));
            _mockUnitOfWork.Setup(u => u.Commit());

            // Act
            _service.UpdateCategory(category);

            // Assert
            _mockCategoryRepository.Verify(r => r.Update(category), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void DeleteCategory_WithNullId_DoesNotCallRepository()
        {
            // Arrange
            int? categoryId = null;

            // Act
            _service.DeleteCategory(categoryId);

            // Assert
            _mockCategoryRepository.Verify(r => r.Remove(It.IsAny<Category>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteCategory_WithValidIdButCategoryNotFound_DoesNotRemove()
        {
            // Arrange
            int categoryId = 1;

            _mockCategoryRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false))
                .Returns((Category?)null);

            // Act
            _service.DeleteCategory(categoryId);

            // Assert
            _mockCategoryRepository.Verify(r => r.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false), Times.Once);
            _mockCategoryRepository.Verify(r => r.Remove(It.IsAny<Category>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteCategory_WithValidIdAndCategoryFound_RemovesAndCommits()
        {
            // Arrange
            int categoryId = 1;
            var category = new Category { Id = categoryId, Name = "Electronics", CreatedBy = "Rifatul" };

            _mockCategoryRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false))
                .Returns(category);
            _mockCategoryRepository.Setup(r => r.Remove(category));
            _mockUnitOfWork.Setup(u => u.Commit());

            // Act
            _service.DeleteCategory(categoryId);

            // Assert
            _mockCategoryRepository.Verify(r => r.Get(It.IsAny<Expression<Func<Category, bool>>>(), null, false), Times.Once);
            _mockCategoryRepository.Verify(r => r.Remove(category), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }
    }
}