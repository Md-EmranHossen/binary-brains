using Microsoft.AspNetCore.Mvc;

namespace ECommerceSystem.API.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
