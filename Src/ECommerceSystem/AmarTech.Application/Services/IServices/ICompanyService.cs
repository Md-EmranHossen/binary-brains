using AmarTech.Domain.Entities;

namespace AmarTech.Application.Services.IServices
{
    public interface ICompanyService
    {
        IEnumerable<Company> GetAllCompanies();
        Company? GetCompanyById(int? id);
        void AddCompany(Company company);
        void UpdateCompany(Company company);
        void DeleteCompany(int? id);
    }
}
