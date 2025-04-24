using AmarTech.Domain.Entities;
using AmarTech.Infrastructure.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AmarTech.Infrastructure.Repository
{
    public  class CategoryRepositroy : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
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
