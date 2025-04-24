using Microsoft.AspNetCore.Mvc;
using AmarTech.Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using AmarTech.Domain.Entities;

namespace AmarTech.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IProductService productService, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            var productList = _productService.GetAllProducts();
            return View(productList);
        }

        public IActionResult Create()
        {
            var categoryList = _productService.CategoryList();
            ViewBag.CategoryList = categoryList;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Product obj, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                return View(obj);
            }

            var wwwRootPath = _webHostEnvironment.WebRootPath;
            _productService.CreatePathOfProduct(obj, file, wwwRootPath);

            _productService.AddProduct(obj);
            TempData["success"] = "Product created successfully";
            return RedirectToAction(nameof(Index));
        }

        private IActionResult LoadProductViewWithCategories(int? id, string viewName)
        {
            if (id is null || id == 0)
            {
                return NotFound();
            }

            var productFromDb = _productService.GetProductById(id);
            if (productFromDb == null)
            {
                return NotFound();
            }

            var categoryList = _productService.CategoryList();
            ViewBag.CategoryList = categoryList;

            return View(viewName, productFromDb);
        }

        public IActionResult Edit(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return LoadProductViewWithCategories(id, "Edit");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Product obj, IFormFile? file)
        {
            if (!ModelState.IsValid)
            {
                return View(obj);
            }

            var wwwRootPath = _webHostEnvironment.WebRootPath;
            _productService.EditPathOfProduct(obj, file, wwwRootPath);
            obj.UpdatedDate = DateTime.Now;
            _productService.UpdateProduct(obj);
            TempData["success"] = "Product updated successfully";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return LoadProductViewWithCategories(id, "Delete");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePost(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id is null)
            {
                return NotFound();
            }

            _productService.DeleteProduct(id);
            TempData["success"] = "Product deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
