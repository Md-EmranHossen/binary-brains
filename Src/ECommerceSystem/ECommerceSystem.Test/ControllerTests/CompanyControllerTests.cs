using ECommerceWebApp.Areas.Admin.Controllers;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Http;
using Moq;


namespace ECommerceSystem.Test.ControllerTests
{
    public class CompanyControllerTests
    {
        private readonly Mock<ICompanyService> _mockCompanyService;
        private readonly CompanyController _controller;

        public CompanyControllerTests()
        {
            _mockCompanyService = new Mock<ICompanyService>();

            _controller = new CompanyController(_mockCompanyService.Object);

            // Setup TempData for the controller
            _controller.TempData = new TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public void Index_ReturnsViewWithCompanyList()
        {
            // Arrange
            var expectedCompanies = new List<Company>
            {
                new Company { Id = 1, Name = "Company A" },
                new Company { Id = 2, Name = "Company B" }
            };
            _mockCompanyService.Setup(s => s.GetAllCompanies()).Returns(expectedCompanies);

            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<Company>>(viewResult.Model);
            Assert.Equal(expectedCompanies.Count, model.Count());
            Assert.Equal(expectedCompanies, model);
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
        public void Create_Post_InvalidModelState_ReturnsViewWithSameModel()
        {
            // Arrange
            var company = new Company { Id = 0, Name = "Test Company" };
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.Create(company);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Company>(viewResult.Model);
            Assert.Equal(company, model);
        }

        [Fact]
        public void Create_Post_ValidModelState_CreatesCompanyAndRedirects()
        {
            // Arrange
            var company = new Company { Id = 0, Name = "New Company" };

            // Act
            var result = _controller.Create(company);

            // Assert
            _mockCompanyService.Verify(s => s.AddCompany(company), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Company created successfully", _controller.TempData["success"]);
        }

        [Fact]
        public void LoadCompanyView_IdIsNullOrZero_ReturnsNotFound()
        {
            // Call the private method using reflection
            var method = typeof(CompanyController).GetMethod("LoadCompanyView",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Test with null id
            var result1 = method?.Invoke(_controller, new object[] { null, "Edit" }) as IActionResult;
            Assert.IsType<NotFoundResult>(result1);

            // Test with zero id
            var result2 = method?.Invoke(_controller, new object[] { 0, "Edit" }) as IActionResult;
            Assert.IsType<NotFoundResult>(result2);
        }

        [Fact]
        public void LoadCompanyView_CompanyNotFound_ReturnsNotFound()
        {
            // Arrange
            int companyId = 99;
            _mockCompanyService.Setup(s => s.GetCompanyById(companyId)).Returns((Company?)null);

            // Call the private method using reflection
            var method = typeof(CompanyController).GetMethod("LoadCompanyView",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = method?.Invoke(_controller, new object[] { companyId, "Edit" }) as IActionResult;

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void LoadCompanyView_CompanyFound_ReturnsViewWithCompany()
        {
            // Arrange
            int companyId = 1;
            var company = new Company { Id = companyId, Name = "Test Company" };

            _mockCompanyService.Setup(s => s.GetCompanyById(companyId)).Returns(company);

            // Call the private method using reflection
            var method = typeof(CompanyController).GetMethod("LoadCompanyView",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Act
            var result = method?.Invoke(_controller, new object[] { companyId, "TestView" }) as IActionResult;

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("TestView", viewResult.ViewName);
            Assert.Equal(company, viewResult.Model);
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
        public void Edit_Get_ValidModelState_CallsLoadCompanyView()
        {
            // Arrange
            int companyId = 1;
            var company = new Company { Id = companyId, Name = "Test Company" };

            _mockCompanyService.Setup(s => s.GetCompanyById(companyId)).Returns(company);

            // Act
            var result = _controller.Edit(companyId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Edit", viewResult.ViewName);
            Assert.Equal(company, viewResult.Model);
        }

        [Fact]
        public void Edit_Post_InvalidModelState_ReturnsViewWithSameModel()
        {
            // Arrange
            var company = new Company { Id = 1, Name = "Updated Company" };
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.Edit(company);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<Company>(viewResult.Model);
            Assert.Equal(company, model);
        }

        [Fact]
        public void Edit_Post_ValidModelState_UpdatesCompanyAndRedirects()
        {
            // Arrange
            var company = new Company { Id = 1, Name = "Updated Company" };

            // Act
            var result = _controller.Edit(company);

            // Assert
            _mockCompanyService.Verify(s => s.UpdateCompany(company), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Company updated successfully", _controller.TempData["success"]);
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
        public void Delete_Get_ValidModelState_CallsLoadCompanyView()
        {
            // Arrange
            int companyId = 1;
            var company = new Company { Id = companyId, Name = "Test Company" };

            _mockCompanyService.Setup(s => s.GetCompanyById(companyId)).Returns(company);

            // Act
            var result = _controller.Delete(companyId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Delete", viewResult.ViewName);
            Assert.Equal(company, viewResult.Model);
        }

        [Fact]
        public void DeleteConfirmed_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Test error");

            // Act
            var result = _controller.DeleteConfirmed(1);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void DeleteConfirmed_IdIsNull_ReturnsNotFound()
        {
            // Act
            var result = _controller.DeleteConfirmed(null);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void DeleteConfirmed_ValidIdProvided_DeletesCompanyAndRedirects()
        {
            // Arrange
            int companyId = 1;

            // Act
            var result = _controller.DeleteConfirmed(companyId);

            // Assert
            _mockCompanyService.Verify(s => s.DeleteCompany(companyId), Times.Once);

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Company deleted successfully", _controller.TempData["success"]);
        }
    }
}