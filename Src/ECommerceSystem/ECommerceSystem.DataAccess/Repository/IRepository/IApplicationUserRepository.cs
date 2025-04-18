﻿using ECommerceSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ECommerceSystem.DataAccess.Repository.IRepository.IRepository;

namespace ECommerceSystem.DataAccess.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
       string GetUserRole(string userId);
        void Update(ApplicationUser applicationUser);
        IEnumerable<SelectListItem> GetAllRoles();
    }
}
