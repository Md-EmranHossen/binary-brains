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
  public  class CompanyRepository : Repository<Company>, ICompanyRepositroy
    {
        private ApplicationDbContext _db;
        public CompanyRepository(ApplicationDbContext db) : base(db) 
        {
            _db = db;
        }

     

        public void Update(Company obj)
        {
            _db.Categories.Update(obj);
        }
    }
}
