using ECommerceSystem.Models;

namespace ECommerceSystem.Service.Services.IServices
{
    public interface IShoppingCartService
    {

        ShoppingCart GetShoppingCartById(int? id);
        void AddShoppingCart(ShoppingCart ShoppingCart);
        void UpdateShoppingCart(ShoppingCart ShoppingCart);
        ShoppingCart GetShoppingCartByUserAndProduct(string userId, int productId);
        IEnumerable<ShoppingCart> GetShoppingCartsByUserId(string userId);
        void RemoveRange(List<ShoppingCart> shoppingCarts);

        void DeleteShoppingCart(int? id);
    }
}
