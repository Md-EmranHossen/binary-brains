using AmarTech.Application.Services.IServices;
using AmarTech.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AmarTech.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {
        private readonly IProductService _productService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IOrderHeaderService _orderHeaderService;

        public DashboardController(IProductService productService,IApplicationUserService applicationUserService,IOrderHeaderService orderHeaderService) {
            _productService = productService;
            _applicationUserService = applicationUserService;
            _orderHeaderService = orderHeaderService;
        }
        public IActionResult Index()
        {
            var dashboardVM= new DashboardVM()
            {
                TotalUsers=_applicationUserService.GetAllUsersCount(),
                TotalOrders=_orderHeaderService.GetAllOrderHeadersCount(),
                TotalProducts=_productService.GetAllProductsCount(),
            };
            return View(dashboardVM);
        }
    }
}
