using AmarTech.Infrastructure;
using AmarTech.Infrastructure.Repository;
using AmarTech.Domain.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using Xunit;

namespace AmarTech.Test.RepositoryTests
{
    public class CompanyRepositoryTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CompanyRepository _repository;

        public CompanyRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;

            _context = new ApplicationDbContext(options);
            _repository = new CompanyRepository(_context);
        }

        [Fact]
        public async Task Update_UpdatesCompany_WhenCompanyExists()
        {
            // Arrange
            var company = new Company
            {
                Id = 1,
                Name = "Test Company",
                StreetAddress = "123 Test St",
                City = "Test City",
                State = "TS",
                PostalCode = "12345",
                PhoneNumber = "123-456-7890"
            };
            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();

            // Act
            company.Name = "Updated Company";
            company.City = "New City";
            _repository.Update(company);
            await _context.SaveChangesAsync();

            // Assert
            var updatedCompany = await _context.Companies.FindAsync(1);
            updatedCompany.Should().NotBeNull();
            updatedCompany.Name.Should().Be("Updated Company");
            updatedCompany.City.Should().Be("New City");
            updatedCompany.StreetAddress.Should().Be("123 Test St"); // Unchanged value
        }

        [Fact]
        public async Task Update_ShouldNotUpdateOtherCompanies()
        {
            // Arrange
            var company1 = new Company
            {
                Id = 1,
                Name = "Company One",
                StreetAddress = "123 First St",
                City = "First City",
                State = "FS",
                PostalCode = "12345",
                PhoneNumber = "123-456-7890"
            };

            var company2 = new Company
            {
                Id = 2,
                Name = "Company Two",
                StreetAddress = "456 Second St",
                City = "Second City",
                State = "SS",
                PostalCode = "67890",
                PhoneNumber = "987-654-3210"
            };

            await _context.Companies.AddRangeAsync(company1, company2);
            await _context.SaveChangesAsync();

            // Act
            company1.Name = "Updated Company One";
            _repository.Update(company1);
            await _context.SaveChangesAsync();

            // Assert
            var updatedCompany1 = await _context.Companies.FindAsync(1);
            var unchangedCompany2 = await _context.Companies.FindAsync(2);

            updatedCompany1?.Name.Should().Be("Updated Company One");
            unchangedCompany2?.Name.Should().Be("Company Two"); // Should remain unchanged
        }

        [Fact]
        public async Task Update_ShouldUpdateAllProvidedProperties()
        {
            // Arrange
            var company = new Company
            {
                Id = 1,
                Name = "Test Company",
                StreetAddress = "123 Test St",
                City = "Test City",
                State = "TS",
                PostalCode = "12345",
                PhoneNumber = "123-456-7890"
            };
            await _context.Companies.AddAsync(company);
            await _context.SaveChangesAsync();

            // Act
            company.Name = "Updated Company";
            company.StreetAddress = "456 New St";
            company.City = "New City";
            company.State = "NS";
            company.PostalCode = "67890";
            company.PhoneNumber = "987-654-3210";

            _repository.Update(company);
            await _context.SaveChangesAsync();

            // Assert
            var updatedCompany = await _context.Companies.FindAsync(1);
            updatedCompany.Should().NotBeNull();
            updatedCompany.Name.Should().Be("Updated Company");
            updatedCompany.StreetAddress.Should().Be("456 New St");
            updatedCompany.City.Should().Be("New City");
            updatedCompany.State.Should().Be("NS");
            updatedCompany.PostalCode.Should().Be("67890");
            updatedCompany.PhoneNumber.Should().Be("987-654-3210");
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}