using ECommerceWebApp.Services;
using ECommerceSystem.DataAccess.Repository;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ECommerceSystem.Service.Services.IServices;

namespace ECommerceWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {

        private readonly IProductService productService;
        private readonly IWebHostEnvironment webHostEnvironment;

        public ProductController(IProductService productService, IWebHostEnvironment webHostEnvironment)
        {
        
            this.productService = productService;
            this.webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var productList = productService.GetAllProducts();
          
            return View(productList);
        }
        public IActionResult Create()
        {

            var CategoryList = productService.CategoryList();

            ViewBag.CategoryList = CategoryList;
            return View();
        }
        [HttpPost]
        public IActionResult Create(Product obj,IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                var wwwRootPath = webHostEnvironment.WebRootPath;
                productService.CreatePathOfProduct(obj, file, wwwRootPath);

                productService.AddProduct(obj);
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
            var productFromDb = productService.GetProductById(id);
            if (productFromDb == null)
            {
                return NotFound();
            }
            var CategoryList = productService.CategoryList();

            ViewBag.CategoryList = CategoryList;
            return View(productFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Product obj, IFormFile? file)
        {
            if (ModelState.IsValid)
            {
                var wwwRootPath = webHostEnvironment.WebRootPath;
                productService.EditPathOfProduct(obj, file,wwwRootPath);
                obj.UpdatedDate = DateTime.Now;
                productService.UpdateProduct(obj);
                TempData["success"] = "Product updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            var productFromDb =productService.GetProductById(id);
            if (productFromDb == null)
            {
                return NotFound();
            }

            var CategoryList = productService.CategoryList();
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
            TempData["success"] = "Product deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
