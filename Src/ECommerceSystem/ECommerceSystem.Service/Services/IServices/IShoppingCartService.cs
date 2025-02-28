using ECommerceSystem.Models;

namespace ECommerceSystem.Service.Services.IServices
{
    public interface IShoppingCartService
    {
        IEnumerable<ShoppingCart> GetAllShoppingCarts();
        ShoppingCart GetShoppingCartById(int? id);
        void AddShoppingCart(ShoppingCart ShoppingCart);
        void UpdateShoppingCart(ShoppingCart ShoppingCart);
        void DeleteShoppingCart(int? id);
    }
}
