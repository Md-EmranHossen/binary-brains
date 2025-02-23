

using ECommerceSystem.DataAccess;
using ECommerceSystem.DataAccess.Repository;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceWebApp.Services
{
        public class ProductService : IProductService
        {
        private readonly IProductRepository productRepositroy;
        private readonly ICategoryRepository categoryRepository;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductService(IProductRepository productRepositroy,ICategoryRepository categoryRepository, IWebHostEnvironment webHostEnvironment)
            {
 
            this.productRepositroy = productRepositroy;
            this.categoryRepository = categoryRepository;
            this.webHostEnvironment = webHostEnvironment;
        }

            public IEnumerable<Product> GetAllProducts()
        {
                return productRepositroy.GetAll(includeProperties: "Category");
            }

            public Product GetProductById(int? id)
            {
                return productRepositroy.Get(u => u.Id == id);
            }

            public void AddProduct(Product Product)
            {
                productRepositroy.Add(Product);
            
            }

            public void UpdateProduct(Product Product)
            {
                productRepositroy.Update(Product);
         
            }

            public void DeleteProduct(int? id)
            {
                var Product = GetProductById(id);
                if (Product != null)
                {
                    productRepositroy.Remove(Product);
    
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

        public void EditPathOfProduct(Product obj, IFormFile? file) {
            string wwwRootPath = webHostEnvironment.WebRootPath;

            if (file != null)
            {
                var oldPath = Path.Combine(wwwRootPath, obj.ImageUrl.TrimStart('\\'));//ImageUrl in database has a \ in front so we need to trim it 1st to get the acutal path
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\product");

                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                obj.ImageUrl = @"\images\product\" + fileName;

            }



        }

        public void CreatePathOfProduct(Product obj, IFormFile? file)
        {
            string wwwRootPath = webHostEnvironment.WebRootPath;

            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string productPath = Path.Combine(wwwRootPath, @"images\product");

                using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                obj.ImageUrl = @"\images\product\" + fileName;

            }
        }
    }
    }
