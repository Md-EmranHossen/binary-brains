using ECommerceSystem.Models;

namespace ECommerceSystem.Service.Services.IServices
{
    public interface IApplicationUserService
    {
     ApplicationUser? GetUserById(string id);  
    }
}
