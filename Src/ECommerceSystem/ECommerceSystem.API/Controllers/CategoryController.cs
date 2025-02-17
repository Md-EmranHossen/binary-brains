using ECommerceSystem.API.Data;
using ECommerceSystem.API.Models;
using ECommerceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceSystem.API.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> categoryList = _categoryService.GetAllCategories();
            return View(categoryList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (ModelState.IsValid)
            {
                _categoryService.AddCategory(obj);
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Edit(int? id)
        {
            if(id == null || id == 0)
            {
                return NotFound();
            }
            Category categoryFromDb = _categoryService.GetCategoryById(id.Value);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {
            if (ModelState.IsValid)
            {
                obj.UpdatedDate = DateTime.Now;
                _categoryService.UpdateCategory(obj);
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            var categoryFromDb = _categoryService.GetCategoryById(id.Value);
            if (categoryFromDb == null)
            {
                return NotFound();
            }
            return View(categoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            _categoryService.DeleteCategory(id.Value);
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
