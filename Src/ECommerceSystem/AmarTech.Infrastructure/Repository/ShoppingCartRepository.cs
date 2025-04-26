using AmarTech.Domain.Entities;
using AmarTech.Infrastructure.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmarTech.Infrastructure.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(ShoppingCart obj)
        {
            _db.ShoppingCarts.Update(obj);
        }
        public void CombineToDB(List<ShoppingCart> cartFromDb, List<ShoppingCart> cartFromMemory,string userId)
        {
            foreach (var cart in cartFromDb)
            {
                var memoryCart=cartFromMemory.FirstOrDefault(u=>u.ProductId==cart.ProductId);
                if (memoryCart == null) continue;

                
                cart.Count+=memoryCart.Count;
                Update(cart);
                _db.SaveChanges();
                cartFromMemory.Remove(memoryCart);



            }
            foreach(var memoryCart in cartFromMemory)
            {
                memoryCart.ApplicationUserId=userId;
                memoryCart.Product = null!;
                Add(memoryCart);
                _db.SaveChanges();
            }

        }

    }
}
