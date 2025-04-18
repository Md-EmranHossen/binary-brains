﻿using ECommerceSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceSystem.Service.Services.IServices
{
    public interface IApplicationUserService
    {
     ApplicationUser? GetUserById(string? id);
     
     IEnumerable<ApplicationUser> GetAllUsers();

        string GetUserrole(string userId);

        void UpdateUser(ApplicationUser user);
        ApplicationUser? GetUserByIdAndIncludeprop(string? id, string includeprop);
        IEnumerable<SelectListItem> GetAllRoles();


    }
}
