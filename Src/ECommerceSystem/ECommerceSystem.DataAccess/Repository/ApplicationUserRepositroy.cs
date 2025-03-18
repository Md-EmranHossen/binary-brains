using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
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
        private ApplicationDbContext _db;
        public ApplicationUserRepositroy(ApplicationDbContext db) : base(db) 
        {
            _db = db;
        }
    }
}
