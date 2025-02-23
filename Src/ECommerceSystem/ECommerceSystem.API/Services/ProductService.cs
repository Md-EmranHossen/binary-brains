

using ECommerceSystem.DataAccess;
using ECommerceSystem.DataAccess.Repository;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceWebApp.Services
{
        public class ProductService : IProductService
        {
        private readonly IProductRepository ProductRepositroy;
        private readonly ICategoryRepository categoryRepository;

        public ProductService(IProductRepository ProductRepositroy,ICategoryRepository categoryRepository)
            {
 
            this.ProductRepositroy = ProductRepositroy;
            this.categoryRepository = categoryRepository;
        }

            public IEnumerable<Product> GetAllProducts()
        {
                return ProductRepositroy.GetAll(includeProperties: "Category");
            }

            public Product GetProductById(int? id)
            {
                return ProductRepositroy.Get(u => u.Id == id);
            }

            public void AddProduct(Product Product)
            {
                ProductRepositroy.Add(Product);
            
            }

            public void UpdateProduct(Product Product)
            {
                ProductRepositroy.Update(Product);
         
            }

            public void DeleteProduct(int? id)
            {
                var Product = GetProductById(id);
                if (Product != null)
                {
                    ProductRepositroy.Remove(Product);
    
                }
            }

        public IEnumerable<SelectListItem> CategoryList()
        {
            return categoryRepository.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.Id.ToString()
            });
        }
    }
    }
