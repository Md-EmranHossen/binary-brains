using ECommerceSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceWebApp.Services
{
    public interface IProductService
    {
        IEnumerable<Product> GetAllProducts();
        Product GetProductById(int? id);
        void AddProduct(Product Product);
        void UpdateProduct(Product Product);
        void DeleteProduct(int? id);

        IEnumerable<SelectListItem> CategoryList();
    }
}
