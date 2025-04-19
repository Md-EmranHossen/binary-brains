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
  public  class ProductRepositroy : Repository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _db;
        public ProductRepositroy(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

     

        public void Update(Product obj)
        {
            _db.Products.Update(obj);
        }
    }
}
