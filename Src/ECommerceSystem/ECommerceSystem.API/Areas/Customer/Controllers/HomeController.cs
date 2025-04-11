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
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;

        public HomeController(ILogger<HomeController> logger, IProductService productService, IShoppingCartService shoppingCartService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _shoppingCartService = shoppingCartService ?? throw new ArgumentNullException(nameof(shoppingCartService));
        }

        public IActionResult Index()
        {
            var productList = _productService.GetAllProducts();
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (productId <= 0)
            {
                return BadRequest("Invalid product ID.");
            }

            var product = _productService.GetProductByIdwithCategory(productId);
            if (product == null)
            {
                _logger.LogWarning("Product not found. ID: {ProductId}", productId);
                return NotFound("Product not found");
            }

            var cart = _shoppingCartService.CreateCartWithProduct(product);
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (shoppingCart == null || shoppingCart.ProductId <= 0)
            {
                _logger.LogWarning("Invalid shopping cart data submitted.");
                return BadRequest("Invalid ShoppingCart Data");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims.");
                return Unauthorized();
            }

            bool isSuccessful = _shoppingCartService.AddOrUpdateShoppingCart(shoppingCart, userId);

            if (!isSuccessful)
            {
                _logger.LogWarning("Failed to add/update shopping cart for user {UserId}", userId);
                return StatusCode(500, "Failed to update shopping cart.");
            }

            return RedirectToAction(nameof(Index));
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
