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




        public IActionResult Index(string? status) 
        {
            var orderData=_orderHeaderService.GetAllOrderHeaders("ApplicationUser");

            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                orderData = orderData.Where(u => u.PaymentStatus.ToLower() == status.ToLower());
            }


            OrderVM orderVM = new OrderVM()
            {
                orderHeader = orderData
            };

            return View(orderVM);
        }

    }
}
