using AmarTech.Domain.Entities;
using AmarTech.Infrastructure.Repository.IRepository;
using AmarTech.Application.Services;
using Moq; // Ensure this is present
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace AmarTech.Test.ServiceTests
{
    public class CompanyServiceTests
    {
        private readonly Mock<ICompanyRepository> _mockCompanyRepository;
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly CompanyService _service;

        public CompanyServiceTests()
        {
            _mockCompanyRepository = new Mock<ICompanyRepository>(); // Fixed typo: was _mockCompanies
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _service = new CompanyService(_mockCompanyRepository.Object, _mockUnitOfWork.Object);
        }

        [Fact]
        public void GetAllCompanies_ReturnsAllCompaniesFromRepository()
        {
            // Arrange
            var expectedCompanies = new List<Company>
            {
                new Company { Id = 1, Name = "Tech Corp", City = null, PhoneNumber = null, PostalCode = null },
                new Company { Id = 2, Name = "Fashion Inc", City = null, PhoneNumber = null, PostalCode = null },
                new Company { Id = 3, Name = "Book Store", City = null, PhoneNumber = null, PostalCode = null }
            };

            _mockCompanyRepository.Setup(r => r.GetAll(null, null))
                .Returns(expectedCompanies);

            // Act
            var result = _service.GetAllCompanies();

            // Assert
            Assert.Equal(expectedCompanies, result);
            _mockCompanyRepository.Verify(r => r.GetAll(null, null), Times.Once);
        }

        [Fact]
        public void GetCompanyById_WithValidId_ReturnsCompanyFromRepository()
        {
            // Arrange
            int companyId = 1;
            var expectedCompany = new Company { Id = companyId, Name = "Tech Corp", City = null, PhoneNumber = null, PostalCode = null };

            _mockCompanyRepository.Setup(r => r.Get(It.Is<Expression<Func<Company, bool>>>(expr => expr.Compile()(expectedCompany)), null, false))
                .Returns(expectedCompany);

            // Act
            var result = _service.GetCompanyById(companyId);

            // Assert
            Assert.Equal(expectedCompany, result);
            _mockCompanyRepository.Verify(r => r.Get(It.IsAny<Expression<Func<Company, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void GetCompanyById_WithNullId_CallsRepositoryAndReturnsNull()
        {
            // Arrange
            int? companyId = null;

            _mockCompanyRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Company, bool>>>(), null, false))
                .Returns((Company?)null);

            // Act
            var result = _service.GetCompanyById(companyId);

            // Assert
            Assert.Null(result);
            _mockCompanyRepository.Verify(r => r.Get(It.IsAny<Expression<Func<Company, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void GetCompanyById_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            int companyId = 999;

            _mockCompanyRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Company, bool>>>(), null, false))
                .Returns((Company?)null);

            // Act
            var result = _service.GetCompanyById(companyId);

            // Assert
            Assert.Null(result);
            _mockCompanyRepository.Verify(r => r.Get(It.IsAny<Expression<Func<Company, bool>>>(), null, false), Times.Once);
        }

        [Fact]
        public void AddCompany_AddsToRepositoryAndCommits()
        {
            // Arrange
            var company = new Company { Id = 1, Name = "Tech Corp", City = null, PhoneNumber = null, PostalCode = null };

            _mockCompanyRepository.Setup(r => r.Add(company));
            _mockUnitOfWork.Setup(u => u.Commit());

            // Act
            _service.AddCompany(company);

            // Assert
            _mockCompanyRepository.Verify(r => r.Add(company), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void UpdateCompany_UpdatesRepositoryAndCommits()
        {
            // Arrange
            var company = new Company { Id = 1, Name = "Updated Tech Corp", City = null, PhoneNumber = null, PostalCode = null };

            _mockCompanyRepository.Setup(r => r.Update(company));
            _mockUnitOfWork.Setup(u => u.Commit());

            // Act
            _service.UpdateCompany(company);

            // Assert
            _mockCompanyRepository.Verify(r => r.Update(company), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }

        [Fact]
        public void DeleteCompany_WithNullId_DoesNotCallRepository()
        {
            // Arrange
            int? companyId = null;

            // Act
            _service.DeleteCompany(companyId);

            // Assert
            _mockCompanyRepository.Verify(r => r.Get(It.IsAny<Expression<Func<Company, bool>>>(), It.IsAny<string>(), It.IsAny<bool>()), Times.Never);
            _mockCompanyRepository.Verify(r => r.Remove(It.IsAny<Company>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteCompany_WithValidIdButCompanyNotFound_DoesNotRemove()
        {
            // Arrange
            int companyId = 1;

            _mockCompanyRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Company, bool>>>(), null, false))
                .Returns((Company?)null);

            // Act
            _service.DeleteCompany(companyId);

            // Assert
            _mockCompanyRepository.Verify(r => r.Get(It.IsAny<Expression<Func<Company, bool>>>(), null, false), Times.Once);
            _mockCompanyRepository.Verify(r => r.Remove(It.IsAny<Company>()), Times.Never);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Never);
        }

        [Fact]
        public void DeleteCompany_WithValidIdAndCompanyFound_RemovesAndCommits()
        {
            // Arrange
            int companyId = 1;
            var company = new Company { Id = companyId, Name = "Rifatul", City = null, PhoneNumber = null, PostalCode = null };

            _mockCompanyRepository.Setup(r => r.Get(It.IsAny<Expression<Func<Company, bool>>>(), null, false))
                .Returns(company);
            _mockCompanyRepository.Setup(r => r.Remove(company));
            _mockUnitOfWork.Setup(u => u.Commit());

            // Act
            _service.DeleteCompany(companyId);

            // Assert
            _mockCompanyRepository.Verify(r => r.Get(It.IsAny<Expression<Func<Company, bool>>>(), null, false), Times.Once);
            _mockCompanyRepository.Verify(r => r.Remove(company), Times.Once);
            _mockUnitOfWork.Verify(u => u.Commit(), Times.Once);
        }
    }
}