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
            var orders = GetUserOrders();
            if (orders == null) return Unauthorized();

            orders = FilterOrdersByStatus(orders, status);
            return View(orders);
        }

        public IActionResult Details(int id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var orderVM = BuildOrderVM(id);
            if (orderVM == null) return NotFound();

            return View(orderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult Details(OrderVM orderVM)
        {
            if (!ModelState.IsValid) return View(orderVM);

            var updated = UpdateOrderHeaderDetails(orderVM);
            if (!updated) return NotFound();

            return RedirectToAction(nameof(Details), new { id = orderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing(OrderVM orderVM)
        {
            if (!ModelState.IsValid) return View(orderVM);

            _orderHeaderService.UpdateStatus(orderVM.orderHeader.Id, SD.StatusInProcess);
            return RedirectToAction(nameof(Details), new { id = orderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder(OrderVM orderVM)
        {
            if (!ModelState.IsValid) return BadRequest();

            var shipped = ShipOrderInternal(orderVM);
            if (!shipped) return NotFound();

            return RedirectToAction(nameof(Details), new { id = orderVM.orderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder(OrderVM orderVM)
        {
            if (!ModelState.IsValid) return BadRequest();

            var cancelled = CancelOrderInternal(orderVM);
            if (!cancelled) return NotFound();

            return RedirectToAction(nameof(Details), new { id = orderVM.orderHeader.Id });
        }

        [HttpPost]
        public IActionResult PayDetails(OrderVM orderVM)
        {
            if (!ModelState.IsValid) return View(orderVM);

            var session = CreateStripeSession(orderVM.orderHeader.Id);
            if (session == null) return NotFound();

            Response.Headers[HeaderLocation] = session.Url;
            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            if (!ModelState.IsValid) return View(orderHeaderId);

            var confirmed = ConfirmStripePayment(orderHeaderId);
            if (!confirmed) return NotFound();

            return View(orderHeaderId);
        }

        // ------------------- Private helper methods -------------------

        private IEnumerable<OrderHeader>? GetUserOrders()
        {
            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
                return _orderHeaderService.GetAllOrderHeaders(ApplicationUser);

            if (User.Identity is ClaimsIdentity identity &&
                identity.FindFirst(ClaimTypes.NameIdentifier) is Claim userIdClaim &&
                !string.IsNullOrEmpty(userIdClaim.Value))
            {
                return _orderHeaderService.GetAllOrderHeadersById(userIdClaim.Value, ApplicationUser);
            }

            return null;
        }

        private IEnumerable<OrderHeader> FilterOrdersByStatus(IEnumerable<OrderHeader> orders, string? status)
        {
            return status?.ToLower() switch
            {
                "pending" => orders.Where(u => u.PaymentStatus == SD.PaymentStatusPending),
                "inprocess" => orders.Where(u => u.OrderStatus == SD.StatusInProcess),
                "completed" => orders.Where(u => u.OrderStatus == SD.StatusShipped),
                "approved" => orders.Where(u => u.OrderStatus == SD.StatusApproved),
                _ => orders
            };
        }

        private OrderVM? BuildOrderVM(int id)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(id, ApplicationUser);
            if (orderHeader == null) return null;

            return new OrderVM
            {
                orderHeader = orderHeader,
                orderDetails = _orderDetailService.GetAllOrders(id, "Product")
            };
        }

        private bool UpdateOrderHeaderDetails(OrderVM orderVM)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id);
            if (orderHeader == null) return false;

            orderHeader.Name = orderVM.orderHeader.Name;
            orderHeader.PhoneNumber = orderVM.orderHeader.PhoneNumber;
            orderHeader.StreetAddress = orderVM.orderHeader.StreetAddress;
            orderHeader.City = orderVM.orderHeader.City;
            orderHeader.State = orderVM.orderHeader.State;
            orderHeader.PostalCode = orderVM.orderHeader.PostalCode;
            orderHeader.Carrier ??= orderVM.orderHeader.Carrier;
            orderHeader.TrackingNumber ??= orderVM.orderHeader.TrackingNumber;

            _orderHeaderService.UpdateOrderHeader(orderHeader);
            return true;
        }

        private bool ShipOrderInternal(OrderVM orderVM)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id);
            if (orderHeader == null) return false;

            orderHeader.TrackingNumber = orderVM.orderHeader.TrackingNumber;
            orderHeader.Carrier = orderVM.orderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _orderHeaderService.UpdateOrderHeader(orderHeader);
            return true;
        }

        private bool CancelOrderInternal(OrderVM orderVM)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id);
            if (orderHeader == null) return false;

            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved && !string.IsNullOrEmpty(orderHeader.PaymentIntentId))
            {
                var refundService = new RefundService();
                refundService.Create(new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                });
                _orderHeaderService.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _orderHeaderService.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            return true;
        }

        private Session? CreateStripeSession(int orderHeaderId)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderHeaderId, ApplicationUser);
            if (orderHeader == null) return null;

            var orderDetails = _orderDetailService.GetAllOrders(orderHeader.Id, "Product");

            var domain = $"{Request.Scheme}://{Request.Host.Value}/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?id={orderHeader.Id}",
                Mode = "payment",
                LineItems = orderDetails.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions { Name = item.Product?.Title ?? "Product" }
                    },
                    Quantity = item.Count
                }).ToList()
            };

            var sessionService = new SessionService();
            var session = sessionService.Create(options);

            _orderHeaderService.UpdateStripePaymentID(orderHeader.Id, session.Id, session.PaymentIntentId);
            return session;
        }

        private bool ConfirmStripePayment(int orderHeaderId)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderHeaderId);
            if (orderHeader == null) return false;

            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                var session = service.Get(orderHeader.SessionId);

                if (string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
                {
                    _orderHeaderService.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _orderHeaderService.UpdateStatus(orderHeaderId, orderHeader.OrderStatus ?? SD.StatusShipped, SD.PaymentStatusApproved);
                }
            }

            return true;
        }
    }
}
