using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceWebApp.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProductService(
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IUnitOfWork unitOfWork)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Product> GetAllProducts()
        {
            return _productRepository.GetAll(includeProperties: "Category");
        }

        public Product? GetProductById(int? id)
        {
            return id == null ? null : _productRepository.Get(u => u.Id == id);
        }

        public void AddProduct(Product product)
        {
            if (product == null) return;

            _productRepository.Add(product);
            _unitOfWork.Commit();
        }

        public void UpdateProduct(Product product)
        {
            if (product == null) return;

            _productRepository.Update(product);
            _unitOfWork.Commit();
        }

        public void DeleteProduct(int? id)
        {
            if (id == null) return;

            var product = GetProductById(id);
            if (product == null) return;

            _productRepository.Remove(product);
            _unitOfWork.Commit();
        }

        public IEnumerable<SelectListItem> CategoryList()
        {
            return _categoryRepository.GetAll()
                .Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
        }

        public void EditPathOfProduct(Product product, IFormFile? file, string wwwRootPath)
        {
            if (product == null || file == null || string.IsNullOrWhiteSpace(wwwRootPath)) return;

            var oldPath = Path.Combine(wwwRootPath, product.ImageUrl.TrimStart('\\'));
            if (File.Exists(oldPath))
            {
                File.Delete(oldPath);
            }

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var productPath = Path.Combine(wwwRootPath, @"images\product");

            Directory.CreateDirectory(productPath); 

            var fullPath = Path.Combine(productPath, fileName);
            using var fileStream = new FileStream(fullPath, FileMode.Create);
            file.CopyTo(fileStream);

            product.ImageUrl = $@"\images\product\{fileName}";
        }

        public void CreatePathOfProduct(Product product, IFormFile? file, string wwwRootPath)
        {
            if (product == null || file == null || string.IsNullOrWhiteSpace(wwwRootPath)) return;

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var productPath = Path.Combine(wwwRootPath, @"images\product");

            Directory.CreateDirectory(productPath); 

            var fullPath = Path.Combine(productPath, fileName);
            using var fileStream = new FileStream(fullPath, FileMode.Create);
            file.CopyTo(fileStream);

            product.ImageUrl = $@"\images\product\{fileName}";
        }

        public Product? GetProductByIdwithCategory(int id)
        {
            return _productRepository.Get(u => u.Id == id, includeProperties: "Category");
        }
    }
}
