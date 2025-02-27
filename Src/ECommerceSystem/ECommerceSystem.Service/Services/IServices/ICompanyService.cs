using ECommerceSystem.Models;

namespace ECommerceSystem.Service.Services.IServices
{
    public interface ICompanyService
    {
        IEnumerable<Company> GetAllCompanies();
        Company GetCompanyById(int? id);
        void AddCompany(Company company);
        void UpdateCompany(Company company);
        void DeleteCompany(int? id);
    }
}
