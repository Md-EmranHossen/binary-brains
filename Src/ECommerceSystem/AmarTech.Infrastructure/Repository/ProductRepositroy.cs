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
    public class ProductRepositroy : Repository<Product>, IProductRepository
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

        public IEnumerable<Product> SkipAndTake(int productsPerPage,int pageNumber,string ? searchQuery=null)
        {
            IQueryable<Product> query = _db.Products;
            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery=searchQuery.ToLower();
                query=query.Where(u=>u.Title.ToLower().Contains(searchQuery)||u.Description.ToLower().Contains(searchQuery));

            }

            query = query.Skip((pageNumber-1) * productsPerPage).Take(productsPerPage);

            return query.ToList();
         
        }
        public void ReduceStockCount(List<ShoppingCart> cartList)
        {
            foreach (var cart in cartList)
            {
                var product = _db.Products.FirstOrDefault(p => p.Id == cart.ProductId);
                if (product != null)
                {
                    product.StockQuantity -= cart.Count;
                }
            }

        }

        public int GetAllProductsCount()
        {
            return _db.Products.Count();
        }
    }
}
