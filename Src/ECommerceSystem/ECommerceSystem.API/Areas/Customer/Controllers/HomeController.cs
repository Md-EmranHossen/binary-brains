using System.Diagnostics;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWebApp.Areas.Customer.Controllers
{

    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService productService;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork,IProductService productService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            this.productService = productService;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> ProductList = productService.GetAllProducts();
            return View(ProductList);
        }


        public IActionResult Details(int ProductId)
        {
            Product product = productService.GetProductByIdwithCategory(ProductId);
            return View(product);
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
