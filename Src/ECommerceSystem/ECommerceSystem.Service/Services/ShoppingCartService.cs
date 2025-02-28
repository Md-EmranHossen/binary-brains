using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;

namespace ECommerceWebApp.Services
{

    public class ShoppingCartService : IShoppingCartService
        {
        private readonly IShoppingCartRepository ShoppingCartRepositroy;

        public ShoppingCartService(IShoppingCartRepository ShoppingCartRepositroy)
            {
 
            this.ShoppingCartRepositroy = ShoppingCartRepositroy;
        }

            public IEnumerable<ShoppingCart> GetAllShoppingCarts()
            {
                return ShoppingCartRepositroy.GetAll();
            }

            public ShoppingCart GetShoppingCartById(int? id)
            {
                return ShoppingCartRepositroy.Get(u => u.Id == id);
            }

            public void AddShoppingCart(ShoppingCart ShoppingCart)
            {
                ShoppingCartRepositroy.Add(ShoppingCart);
            
            }

            public void UpdateShoppingCart(ShoppingCart ShoppingCart)
            {
                ShoppingCartRepositroy.Update(ShoppingCart);
         
            }

            public void DeleteShoppingCart(int? id)
            {
                var ShoppingCart = GetShoppingCartById(id);
                if (ShoppingCart != null)
                {
                    ShoppingCartRepositroy.Remove(ShoppingCart);
    
                }
            }

        public ShoppingCart GetShoppingCartById(bool v)
        {
            throw new NotImplementedException();
        }
    }
    }
