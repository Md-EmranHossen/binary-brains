using ECommerceSystem.DataAccess.Repository;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceSystem.Service.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;

        public ShoppingCartService(IShoppingCartRepository shoppingCartRepository)
        {
            _shoppingCartRepository = shoppingCartRepository;
        }

        public void AddShoppingCart(ShoppingCart ShoppingCart)
        {
            _shoppingCartRepository.Add(ShoppingCart);
        }

        public void DeleteShoppingCart(int? id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ShoppingCart> GetAllShoppingCarts()
        {
            throw new NotImplementedException();
        }

        public ShoppingCart GetShoppingCartById(int? id)
        {
            return _shoppingCartRepository.Get(u => u.Id == id);

        }

       

        public void UpdateShoppingCart(ShoppingCart ShoppingCart)
        {
            throw new NotImplementedException();
        }
    }
}
