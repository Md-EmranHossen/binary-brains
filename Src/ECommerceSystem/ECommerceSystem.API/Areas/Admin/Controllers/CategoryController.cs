using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using ECommerceSystem.Utility;
using Microsoft.AspNetCore.Authorization;
using ECommerceSystem.Service.Services.IServices;

namespace ECommerceWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICategoryService categoryService;

        public CategoryController(IUnitOfWork unitOfWork,ICategoryService categoryService)
        {
            _unitOfWork = unitOfWork;
            this.categoryService = categoryService;
        }

        public IActionResult Index()
        {
            IEnumerable<Category> categoryList = categoryService.GetAllCategories();
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
                categoryService.AddCategory(obj);
                _unitOfWork.Commit();
                TempData["success"] = "Category created successfully";
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
            Category categoryFromDb = categoryService.GetCategoryById(id);
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
                categoryService.UpdateCategory(obj);
                _unitOfWork.Commit();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            var categoryFromDb = categoryService.GetCategoryById(id);
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

            categoryService.DeleteCategory(id);
            _unitOfWork.Commit();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
