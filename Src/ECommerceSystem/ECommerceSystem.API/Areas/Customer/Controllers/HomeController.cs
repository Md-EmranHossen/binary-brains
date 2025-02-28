using System.Diagnostics;
using System.Security.Claims;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWebApp.Areas.Customer.Controllers
{

    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService productService;
        private readonly IShoppingCartService shoppingCartService;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork, IProductService productService, IShoppingCartService shoppingCartService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            this.productService = productService;
            this.shoppingCartService = shoppingCartService;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> ProductList = productService.GetAllProducts();
            return View(ProductList);
        }


        public IActionResult Details(int ProductId)
        {
            ShoppingCart cart = new()
            {
                Product = productService.GetProductByIdwithCategory(ProductId),
                Count = 1,
                ProductId = ProductId
            };

            return View(cart);
        }


        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = shoppingCartService
            .GetAllShoppingCarts()
            .FirstOrDefault(c => c.ApplicationUserId == shoppingCart.ApplicationUserId && c.ProductId == shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                shoppingCartService.UpdateShoppingCart(cartFromDb); 
            }
            else
            {
                shoppingCartService.AddShoppingCart(shoppingCart);
            }
            

            _unitOfWork.Commit();
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
