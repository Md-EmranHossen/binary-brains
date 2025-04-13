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
        private readonly IOrderDetailService _orderDetailService;

        public OrderController(IOrderHeaderService orderHeaderService,IOrderDetailService orderDetailService)
        {
            _orderHeaderService = orderHeaderService;
            _orderDetailService = orderDetailService;
        }




        public IActionResult Index(string? status) 
        {
            var orderData=_orderHeaderService.GetAllOrderHeaders("ApplicationUser");

            if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
            {
                orderData = orderData.Where(u => u.PaymentStatus.ToLower() == status.ToLower());
            }


            

            return View(orderData);
        }
        public IActionResult Details(int id)
        {
            OrderVM orderVM = new OrderVM()
            {
                orderHeader = _orderHeaderService.GetOrderHeaderById(id, "ApplicationUser"),
                orderDetails=_orderDetailService.GetAllOrders(id,"Product")
            };

            return View(orderVM);
        }

    }
}
