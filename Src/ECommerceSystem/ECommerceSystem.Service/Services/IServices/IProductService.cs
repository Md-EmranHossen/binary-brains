using ECommerceSystem.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceSystem.Service.Services.IServices
{
    public interface IProductService
    {
        IEnumerable<Product> GetAllProducts();
        Product GetProductById(int? id);
        Product GetProductByIdwithCategory(int id);
        void AddProduct(Product Product);
        void UpdateProduct(Product Product);
        void DeleteProduct(int? id);

        IEnumerable<SelectListItem> CategoryList();

        void EditPathOfProduct(Product obj, IFormFile? file,string wwwRootPath);
        void CreatePathOfProduct(Product obj, IFormFile? file, string wwwRootPath);
    }
}
