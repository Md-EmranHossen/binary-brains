using ECommerceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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

        ShoppingCartVM GetShoppingCartVM(string? userId);

        void RemoveShoppingCarts(OrderHeader? orderHeader);

        void Plus(int cartId);

        void Minus(int cartId);

        void RemoveCartValue(int cartId);
        ShoppingCart GetShoppingCartByUserId(string userId);



    }
}
