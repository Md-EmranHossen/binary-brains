using System.Diagnostics;
using System.Security.Claims;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services;
using ECommerceSystem.Service.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWebApp.Areas.Customer.Controllers
{

    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IProductService productService;
        private readonly IShoppingCartService shoppingCartService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, IShoppingCartService shoppingCartService)
        {
            _logger = logger;
            this.productService = productService;
            this.shoppingCartService = shoppingCartService;
        }

        public IActionResult Index()
        {
            var ProductList = productService.GetAllProducts();
            return View(ProductList);
        }

        public IActionResult Details(int ProductId)
        {
            var product = productService.GetProductByIdwithCategory(ProductId);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            var cart = shoppingCartService.CreateCartWithProduct(product);

            return View(cart);
        }

        

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null || shoppingCart == null || shoppingCart.ProductId == 0)
            {
                return BadRequest("Invalid ShoppingCart Data");
            }

            bool isSuccessful = shoppingCartService.AddOrUpdateShoppingCart(shoppingCart, userId);

            if (!isSuccessful)
            {
                return Unauthorized();
            }

            return RedirectToAction("Index");
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
