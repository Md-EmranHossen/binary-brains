using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;

namespace ECommerceWebApp.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository CompanyRepositroy;

        public CompanyService(ICompanyRepository CompanyRepositroy)
            {
 
            this.CompanyRepositroy = CompanyRepositroy;
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
            
            }

            public void UpdateCompany(Company Company)
            {
                CompanyRepositroy.Update(Company);
         
            }

            public void DeleteCompany(int? id)
            {
                var Company = GetCompanyById(id);
                if (Company != null)
                {
                    CompanyRepositroy.Remove(Company);
    
                }
            }
        }
    }
