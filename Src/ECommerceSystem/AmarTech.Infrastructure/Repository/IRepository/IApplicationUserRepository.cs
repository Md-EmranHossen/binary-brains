using AmarTech.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AmarTech.Infrastructure.Repository.IRepository.IRepository;

namespace AmarTech.Infrastructure.Repository.IRepository
{
    public interface IApplicationUserRepository : IRepository<ApplicationUser>
    {
       string GetUserRole(string userId);
        void Update(ApplicationUser applicationUser);
        IEnumerable<SelectListItem> GetAllRoles();
        int GetAllUsersCount();
    }
}
