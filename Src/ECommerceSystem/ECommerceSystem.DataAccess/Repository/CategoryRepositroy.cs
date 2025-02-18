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
  public  class CategoryRepositroy : Repository<Category>, ICategoryRepository
    {
        private ApplicationDbContext _db;
        public CategoryRepositroy(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

     

        public void Update(Category obj)
        {
            _db.Categories.Update(obj);
        }
    }
}
