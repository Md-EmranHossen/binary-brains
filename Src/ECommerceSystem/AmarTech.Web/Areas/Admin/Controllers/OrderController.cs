using AmarTech.Domain.Entities;
using AmarTech.Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace AmarTech.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IOrderHeaderService _orderHeaderService;
        private readonly IOrderDetailService _orderDetailService;
        private const string ApplicationUser = "ApplicationUser";
        private const string HeaderLocation = "Location";

        public OrderController(IOrderHeaderService orderHeaderService, IOrderDetailService orderDetailService)
        {
            _orderHeaderService = orderHeaderService;
            _orderDetailService = orderDetailService;
        }

        public IActionResult Index(string? status)
        {
            var orderData = GetOrderHeaders();
            if (orderData == null)
            {
                return Unauthorized();
            }

            orderData = FilterOrdersByStatus(orderData, status);
            return View(orderData);
        }

        public IActionResult Details(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderVM = CreateOrderViewModel(id);
            if (orderVM == null)
            {
                return NotFound();
            }

            return View(orderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult Details(OrderVM orderVM)
        {
            if (!ModelState.IsValid)
            {
                return View(orderVM);
            }

            var orderHeader = UpdateOrderHeader(orderVM);
            if (orderHeader == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = orderHeader.Id });
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
            return RedirectToAction(nameof(Details), new { id = orderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder(OrderVM orderVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var orderHeader = UpdateShippingDetails(orderVM);
            if (orderHeader == null)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Details), new { id = orderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(OrderVM orderVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var orderHeader = ProcessOrderCancellation(orderVM);
            if (orderHeader == null)
            {
                return NotFound();
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

            var orderVMUpdated = PopulateOrderViewModel(orderVM);
            if (orderVMUpdated == null)
            {
                return NotFound();
            }

            var sessionUrl = CreateStripeSession(orderVMUpdated);
            Response.Headers[HeaderLocation] = sessionUrl;
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            if (!ModelState.IsValid)
            {
                return View(orderHeaderId);
            }

            var orderHeader = ProcessPaymentConfirmation(orderHeaderId);
            if (orderHeader == null)
            {
                return NotFound();
            }

            return View(orderHeaderId);
        }

        #region Helper Methods

        private IEnumerable<OrderHeader>? GetOrderHeaders()
        {
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                return _orderHeaderService.GetAllOrderHeaders(ApplicationUser);
            }

            if (User.Identity is ClaimsIdentity identity && identity.FindFirst(ClaimTypes.NameIdentifier) is Claim userIdClaim)
            {
                return _orderHeaderService.GetAllOrderHeadersById(userIdClaim.Value, ApplicationUser);
            }

            return null;
        }

        private IEnumerable<OrderHeader> FilterOrdersByStatus(IEnumerable<OrderHeader> orderData, string? status)
        {
            return status switch
            {
                "pending" => orderData.Where(u => u.PaymentStatus == SD.PaymentStatusPending),
                "inprocess" => orderData.Where(u => u.OrderStatus == SD.StatusInProcess),
                "completed" => orderData.Where(u => u.OrderStatus == SD.StatusShipped),
                "approved" => orderData.Where(u => u.OrderStatus == SD.StatusApproved),
                _ => orderData
            };
        }

        private OrderVM? CreateOrderViewModel(int id)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(id, ApplicationUser);
            if (orderHeader == null)
            {
                return null;
            }

            return new OrderVM
            {
                orderHeader = orderHeader,
                orderDetails = _orderDetailService.GetAllOrders(id, "Product")
            };
        }

        private OrderHeader? UpdateOrderHeader(OrderVM orderVM)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id);
            if (orderHeader == null)
            {
                return null;
            }

            orderHeader.Name = orderVM.orderHeader.Name;
            orderHeader.PhoneNumber = orderVM.orderHeader.PhoneNumber;
            orderHeader.StreetAddress = orderVM.orderHeader.StreetAddress;
            orderHeader.City = orderVM.orderHeader.City;
            orderHeader.State = orderVM.orderHeader.State;
            orderHeader.PostalCode = orderVM.orderHeader.PostalCode;
            orderHeader.Carrier = orderVM.orderHeader.Carrier ?? orderHeader.Carrier;
            orderHeader.TrackingNumber = orderVM.orderHeader.TrackingNumber ?? orderHeader.TrackingNumber;

            _orderHeaderService.UpdateOrderHeader(orderHeader);
            return orderHeader;
        }

        private OrderHeader? UpdateShippingDetails(OrderVM orderVM)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id);
            if (orderHeader == null)
            {
                return null;
            }

            orderHeader.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            orderHeader.Carrier = orderVM.orderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }

            _orderHeaderService.UpdateOrderHeader(orderHeader);
            return orderHeader;
        }

        private OrderHeader? ProcessOrderCancellation(OrderVM orderVM)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id);
            if (orderHeader == null)
            {
                return null;
            }

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved && orderHeader.PaymentIntentId != null)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };
                new RefundService().Create(options);
                _orderHeaderService.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _orderHeaderService.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            return orderHeader;
        }

        private OrderVM? PopulateOrderViewModel(OrderVM orderVM)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id, ApplicationUser);
            if (orderHeader == null)
            {
                return null;
            }

            orderVM.orderHeader = orderHeader;
            orderVM.orderDetails = _orderDetailService.GetAllOrders(orderHeader.Id, "Product");
            return orderVM;
        }

        private string CreateStripeSession(OrderVM orderVM)
        {
            var domain = $"{Request.Scheme}://{Request.Host.Value}/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderVM.orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?id={orderVM.orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderVM.orderDetails)
            {
                if (item.Product == null)
                {
                    continue;
                }

                options.LineItems.Add(new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                });
            }

            var session = new SessionService().Create(options);
            _orderHeaderService.UpdateStripePaymentID(orderVM.orderHeader.Id, session.Id, session.PaymentIntentId);
            return session.Url;
        }

        private OrderHeader? ProcessPaymentConfirmation(int orderHeaderId)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderHeaderId);
            if (orderHeader == null)
            {
                return null;
            }

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var session = new SessionService().Get(orderHeader.SessionId);
                if (string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
                {
                    _orderHeaderService.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _orderHeaderService.UpdateStatus(orderHeaderId, orderHeader.OrderStatus ?? SD.StatusShipped, SD.PaymentStatusApproved);
                }
            }

            return orderHeader;
        }

        #endregion
    }
}