using AmarTech.Domain.Entities;
using AmarTech.Infrastructure.Repository;
using AmarTech.Infrastructure.Repository.IRepository;
using AmarTech.Application.Services.IServices;

namespace AmarTech.Application.Services
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

        public Company? GetCompanyById(int? id)
        {
            return CompanyRepositroy.Get(u => u.Id == id);
        }

        public void AddCompany(Company company)
        {
            CompanyRepositroy.Add(company);
            _unitOfWork.Commit();

        }

        public void UpdateCompany(Company company)
        {
            CompanyRepositroy.Update(company);
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
