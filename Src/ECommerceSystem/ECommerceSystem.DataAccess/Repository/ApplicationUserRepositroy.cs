using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceSystem.DataAccess.Repository
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
            if(userId == null)
            {
                return "";
            }
            var roleId=_db.UserRoles.FirstOrDefault(x => x.UserId == userId).RoleId;
            var role = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;


            return role;
        }
        public void Update(ApplicationUser obj)
        {
            _db.ApplicationUsers.Update(obj);
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
