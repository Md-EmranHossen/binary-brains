using ECommerceSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceSystem.Service.Services.IServices
{
    public interface IShoppingCartService
    {

        ShoppingCart? GetShoppingCartById(int? id);
        void AddShoppingCart(ShoppingCart shoppingCart);
        void UpdateShoppingCart(ShoppingCart shoppingCart);
        ShoppingCart? GetShoppingCartByUserAndProduct(string userId, int productId);
        IEnumerable<ShoppingCart> GetShoppingCartsByUserId(string userId);
        void RemoveRange(List<ShoppingCart> shoppingCarts);

        void DeleteShoppingCart(int? id);

        ShoppingCart CreateCartWithProduct(Product product);

        bool AddOrUpdateShoppingCart(ShoppingCart shoppingCart, string userId);



    }
}
