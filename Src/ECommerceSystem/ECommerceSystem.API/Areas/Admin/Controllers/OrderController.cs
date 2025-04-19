using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Security.Claims;

namespace ECommerceWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]


    public class OrderController : Controller
    {
        private readonly IOrderHeaderService _orderHeaderService;
        private readonly IOrderDetailService _orderDetailService;
        private const string ApplicationUser = "ApplicationUser";
        public OrderController(IOrderHeaderService orderHeaderService,IOrderDetailService orderDetailService)
        {
            _orderHeaderService = orderHeaderService;
            _orderDetailService = orderDetailService;
        }




        public IActionResult Index(string? status) 
        {
            IEnumerable<OrderHeader> orderData;

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
               orderData = _orderHeaderService.GetAllOrderHeaders(ApplicationUser);
            }

            else
            {
                var claimsIdentity = User.Identity as ClaimsIdentity;

                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;


                orderData=_orderHeaderService.GetAllOrderHeadersById(userId, ApplicationUser);
            }

            switch (status)
            {
                case "pending":
                    orderData = orderData.Where(u => u.PaymentStatus == SD.PaymentStatusPending);
                    break;
                case "inprocess":
                    orderData = orderData.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderData = orderData.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;
                case "approved":
                    orderData = orderData.Where(u => u.OrderStatus == SD.StatusApproved); 
                    break;
                default:
                    break;

            }
            return View(orderData);
        }
        public IActionResult Details(int id)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            OrderVM orderVM = new OrderVM()
            {
                orderHeader = _orderHeaderService.GetOrderHeaderById(id, ApplicationUser),
                orderDetails=_orderDetailService.GetAllOrders(id,"Product")
            };

            return View(orderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin+","+SD.Role_Employee)]
        public IActionResult Details(OrderVM orderVM)
        {



            if (!ModelState.IsValid)
            {
                return View(orderVM);
            }



            var orderHeaderFromDb = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id);

            orderHeaderFromDb.Name = orderVM.orderHeader.Name;
            orderHeaderFromDb.PhoneNumber = orderVM.orderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = orderVM.orderHeader.StreetAddress;
            orderHeaderFromDb.City = orderVM.orderHeader.City;
            orderHeaderFromDb.State = orderVM.orderHeader.State;
            orderHeaderFromDb.PostalCode = orderVM.orderHeader.PostalCode;
            if (!string.IsNullOrEmpty(orderVM.orderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = orderVM.orderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(orderVM.orderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            }
            _orderHeaderService.UpdateOrderHeader(orderHeaderFromDb);
    


            return RedirectToAction(nameof(Details), new { id = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing(OrderVM orderVM)
        {

            if (!ModelState.IsValid)
            {
                return View(orderVM);
            }

            _orderHeaderService.UpdateStatus(orderVM.orderHeader.Id, SD.StatusInProcess);

            return RedirectToAction(nameof(Details), new { id=orderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder(OrderVM orderVM)
        {

            if (!ModelState.IsValid)
            {
                return View(orderVM);
            }

            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id);
            orderHeader.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            orderHeader.Carrier = orderVM.orderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _orderHeaderService.UpdateOrderHeader(orderHeader);

            return RedirectToAction(nameof(Details), new { id = orderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(OrderVM orderVM)
        {
            if (!ModelState.IsValid)
            {
                return View(orderVM);
            }

            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id);

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved && orderHeader.PaymentIntentId!=null)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();

                _orderHeaderService.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _orderHeaderService.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
          
            return RedirectToAction(nameof(Details), new { id = orderVM.orderHeader.Id });

        }
        [HttpPost]
        public IActionResult PayDetails(OrderVM orderVM)
        {

            if (!ModelState.IsValid)
            {
                return View(orderVM);
            }
            orderVM.orderHeader = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id,  ApplicationUser);
            orderVM.orderDetails = _orderDetailService.GetAllOrders(orderVM.orderHeader.Id, "Product");


            //stripe logic
            var domain = Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderVM.orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?id={orderVM.orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderVM.orderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }


            var service = new SessionService();
            Session session = service.Create(options);
            _orderHeaderService.UpdateStripePaymentID(orderVM.orderHeader.Id, session.Id, session.PaymentIntentId);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }
        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            if (!ModelState.IsValid)
            {
                return View(orderHeaderId);
            }

            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderHeaderId);
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //this is an order by company

                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _orderHeaderService.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _orderHeaderService.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                }


            }


            return View(orderHeaderId);
        }





    }
}
