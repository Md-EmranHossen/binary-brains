using AmarTech.Infrastructure.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AmarTech.Application.Services.IServices;
using AmarTech.Domain.Entities;
using System.Security.Claims;

namespace AmarTech.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IApplicationUserService _applicationUserService;

        public CategoryController(ICategoryService categoryService,IApplicationUserService applicationUserService)
        {
            _categoryService = categoryService;
            _applicationUserService = applicationUserService;
        }

        public IActionResult Index()
        {
            var categories = _categoryService.GetAllCategories();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }
            category.CreatedBy = GetCurrentUserName();

            _categoryService.AddCategory(category);
            TempData["success"] = "Category created successfully";
            return RedirectToAction(nameof(Index));
        }

        private IActionResult LoadCategoryView(int? id, string viewName)
        {
            if (id is null || id == 0)
            {
                return NotFound();
            }

            var category = _categoryService.GetCategoryById(id);
            if (category == null)
            {
                return NotFound();
            }

            return View(viewName, category);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return LoadCategoryView(id, "Edit");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View(category);
            }

            category.UpdatedBy = GetCurrentUserName();

            category.UpdatedDate = DateTime.Now;
            _categoryService.UpdateCategory(category);
            TempData["success"] = "Category updated successfully";
            return RedirectToAction(nameof(Index));
        }

  
        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return LoadCategoryView(id, "Delete");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id is null)
            {
                return NotFound();
            }

            _categoryService.DeleteCategory(id);
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction(nameof(Index));
        }

        public string GetCurrentUserName()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return _applicationUserService.GetUserName(userId);
        }
    }
}
