using ECommerceSystem.DataAccess.Repository;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;

namespace ECommerceWebApp.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository CompanyRepositroy;
        private readonly IUnitOfWork _unitOfWork;

        public CompanyService(ICompanyRepository CompanyRepositroy,IUnitOfWork unitOfWork)
        {

            this.CompanyRepositroy = CompanyRepositroy;
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Company> GetAllCompanies()
        {
            return CompanyRepositroy.GetAll();
        }

        public Company GetCompanyById(int? id)
        {
            return CompanyRepositroy.Get(u => u.Id == id);
        }

        public void AddCompany(Company Company)
        {
            CompanyRepositroy.Add(Company);
            _unitOfWork.Commit();

        }

        public void UpdateCompany(Company Company)
        {
            CompanyRepositroy.Update(Company);
            _unitOfWork.Commit();

        }

        public void DeleteCompany(int? id)
        {
            if (id != null)
            {
                var Company = GetCompanyById(id);
                if (Company != null)
                {
                    CompanyRepositroy.Remove(Company);
                    _unitOfWork.Commit();

                }
            }
        }
    }
}
