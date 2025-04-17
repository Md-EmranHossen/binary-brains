using ECommerceSystem.Models;

namespace ECommerceSystem.Service.Services.IServices
{
    public interface IApplicationUserService
    {
     ApplicationUser? GetUserById(string? id);
     
     IEnumerable<ApplicationUser> GetAllUsers();

        string GetUserrole(string userId);

        void UpdateUser(ApplicationUser user);


    }
}
