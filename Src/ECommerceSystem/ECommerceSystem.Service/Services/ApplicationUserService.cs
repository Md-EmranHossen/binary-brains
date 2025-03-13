using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;

namespace ECommerceWebApp.Services
{
    public class ApplicationUserService : IApplicationUserService
        {
        private readonly IApplicationUserRepository ApplicationUserRepositroy;

        public ApplicationUserService(IApplicationUserRepository ApplicationUserRepositroy)
            {
 
            this.ApplicationUserRepositroy = ApplicationUserRepositroy;
        }

        public void AddApplicationUser(ApplicationUser ApplicationUser)
        {
            throw new NotImplementedException();
        }

        public void DeleteApplicationUser(int? id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ApplicationUser> GetAllCategories()
        {
            throw new NotImplementedException();
        }

        public ApplicationUser GetApplicationUserById(int? id)
        {
            throw new NotImplementedException();
        }

        public void UpdateApplicationUser(ApplicationUser ApplicationUser)
        {
            throw new NotImplementedException();
        }
    }
    }
