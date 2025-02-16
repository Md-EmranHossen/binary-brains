using ECommerceSystem.API.Data;
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
            return View();
        }
    }
}
