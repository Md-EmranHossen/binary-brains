using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly IOrderHeaderService _orderHeaderService;

        public OrderController(IOrderHeaderService orderHeaderService)
        {
            _orderHeaderService = orderHeaderService;
        }




        public IActionResult Index()
        {
            var orderData=_orderHeaderService.GetAllOrderHeaders("ApplicationUser");

            OrderVM orderVM = new OrderVM()
            {
                orderHeader = orderData
            };

            return View(orderVM);
        }
    }
}
