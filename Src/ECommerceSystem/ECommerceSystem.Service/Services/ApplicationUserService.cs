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

        ApplicationUser? IApplicationUserService.GetUserById(string? id)
        {
            return ApplicationUserRepositroy.Get(u => u.Id == id);
        }
    }
}
