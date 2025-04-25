using AmarTech.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AmarTech.Application.Services.IServices
{
    public interface IApplicationUserService
    {
     ApplicationUser? GetUserById(string? id);
     
     IEnumerable<ApplicationUser> GetAllUsers();

        string GetUserrole(string userId);

        void UpdateUser(ApplicationUser user);
        ApplicationUser? GetUserByIdAndIncludeprop(string? id, string includeprop);
        IEnumerable<SelectListItem> GetAllRoles();
        int GetAllUsersCount();


    }
}
