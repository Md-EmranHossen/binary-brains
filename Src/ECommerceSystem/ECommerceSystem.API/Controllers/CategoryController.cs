using ECommerceSystem.API.Data;
using ECommerceSystem.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceSystem.API.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            List<Category> categoryList = _db.Categories.ToList();
            return View(categoryList);
        }
    }
}
