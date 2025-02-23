using ECommerceWebApp.Services;
using ECommerceSystem.DataAccess.Repository;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService productService;

        public ProductController(IUnitOfWork unitOfWork,IProductService productService)
        {
            _unitOfWork = unitOfWork;
        
            this.productService = productService;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = productService.GetAllProducts();
          
            return View(productList);
        }
        public IActionResult Create()
        {

            IEnumerable<SelectListItem> CategoryList = productService.CategoryList();

            ViewBag.CategoryList = CategoryList;
            return View();
        }
        [HttpPost]
        public IActionResult Create(Product obj,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                productService.CreatePathOfProduct(obj, file);

                productService.AddProduct(obj);
                _unitOfWork.Commit();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product productFromDb = productService.GetProductById(id);
            if (productFromDb == null)
            {
                return NotFound();
            }
            IEnumerable<SelectListItem> CategoryList = productService.CategoryList();

            ViewBag.CategoryList = CategoryList;
            return View(productFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Product obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                productService.EditPathOfProduct(obj, file);
                obj.UpdatedDate = DateTime.Now;
                productService.UpdateProduct(obj);
                _unitOfWork.Commit();
                TempData["success"] = "Product updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            Product productFromDb =productService.GetProductById(id);
            if (productFromDb == null)
            {
                return NotFound();
            }

            IEnumerable<SelectListItem> CategoryList = productService.CategoryList();
            ViewBag.CategoryList = CategoryList;
            return View(productFromDb);

        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
             productService.DeleteProduct(id);
            _unitOfWork.Commit();
            TempData["success"] = "Product deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
