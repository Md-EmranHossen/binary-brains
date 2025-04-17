using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;

namespace ECommerceWebApp.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IApplicationUserRepository _applicationUserRepositroy;
        public ApplicationUserService(IApplicationUserRepository ApplicationUserRepositroy)
        {
            _applicationUserRepositroy = ApplicationUserRepositroy;
        }

        public IEnumerable<ApplicationUser> GetAllUsers()
        {
            var obj= _applicationUserRepositroy.GetAll(includeProperties: "Company");
            return obj;
        }

        public string GetUserrole(string userId)
        {
           return _applicationUserRepositroy.GetUserRole(userId);
        }

        ApplicationUser? IApplicationUserService.GetUserById(string? id)
        {
            return _applicationUserRepositroy.Get(u => u.Id == id);
        }

    }
}
