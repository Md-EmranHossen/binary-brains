using ECommerceSystem.Models;

namespace ECommerceSystem.Service.Services.IServices
{
    public interface IApplicationUserService
    {
        IEnumerable<ApplicationUser> GetAllCategories();
        ApplicationUser GetApplicationUserById(int? id);
        void AddApplicationUser(ApplicationUser ApplicationUser);
        void UpdateApplicationUser(ApplicationUser ApplicationUser);
        void DeleteApplicationUser(int? id);
    }
}
