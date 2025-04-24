using AmarTech.Domain.Entities;
using AmarTech.Infrastructure.Repository.IRepository;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AmarTech.Infrastructure.Repository
{
    public  class ApplicationUserRepositroy : Repository<ApplicationUser>, IApplicationUserRepository
    {
        private readonly ApplicationDbContext _db;

        public ApplicationUserRepositroy(ApplicationDbContext db) : base(db) 
        {
            _db = db;
        }

        public string GetUserRole(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return string.Empty;
            }

            var userRole = _db.UserRoles.FirstOrDefault(x => x.UserId == userId);
            if (userRole == null)
            {
                return string.Empty;
            }

            var role = _db.Roles.FirstOrDefault(u => u.Id == userRole.RoleId);
            if (role == null)
            {
                return string.Empty;
            }

            return role.Name ?? string.Empty;
        }

        public void Update(ApplicationUser applicationUser)
        {
            _db.ApplicationUsers.Update(applicationUser);
        }

        public IEnumerable<SelectListItem> GetAllRoles()
        {
            return _db.Roles.Select(i => new SelectListItem
            {
                Text = i.Name,
                Value = i.Name,


            });
        }



    }
}
