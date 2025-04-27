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
            if (IsUserInAdminOrEmployeeRole())
            {
                return _orderHeaderService.GetAllOrderHeaders(ApplicationUser);
            }

            var userId = GetUserId();
            if (userId != null)
            {
                return _orderHeaderService.GetAllOrderHeadersById(userId, ApplicationUser);
            }

            return null;
        }

        private bool IsUserInAdminOrEmployeeRole()
        {
            return User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee);
        }

        private string? GetUserId()
        {
            if (User.Identity is not ClaimsIdentity identity)
            {
                return null;
            }

            var userIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim?.Value;
        }

        private IEnumerable<OrderHeader> FilterOrdersByStatus(IEnumerable<OrderHeader> orderData, string? status)
        {
            if (status == null)
            {
                return orderData;
            }

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

            UpdateOrderHeaderFields(orderHeader, orderVM.orderHeader);
            _orderHeaderService.UpdateOrderHeader(orderHeader);
            return orderHeader;
        }

        private void UpdateOrderHeaderFields(OrderHeader orderHeader, OrderHeader updatedHeader)
        {
            orderHeader.Name = updatedHeader.Name;
            orderHeader.PhoneNumber = updatedHeader.PhoneNumber;
            orderHeader.StreetAddress = updatedHeader.StreetAddress;
            orderHeader.City = updatedHeader.City;
            orderHeader.State = updatedHeader.State;
            orderHeader.PostalCode = updatedHeader.PostalCode;

            // Only update if not null
            if (updatedHeader.Carrier != null)
            {
                orderHeader.Carrier = updatedHeader.Carrier;
            }

            if (updatedHeader.TrackingNumber != null)
            {
                orderHeader.TrackingNumber = updatedHeader.TrackingNumber;
            }
        }

        private OrderHeader? UpdateShippingDetails(OrderVM orderVM)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id);
            if (orderHeader == null)
            {
                return null;
            }

            UpdateTrackingInformation(orderHeader, orderVM.orderHeader);
            UpdateOrderStatusForShipping(orderHeader);
            UpdatePaymentDueDateIfNeeded(orderHeader);

            _orderHeaderService.UpdateOrderHeader(orderHeader);
            return orderHeader;
        }

        private void UpdateTrackingInformation(OrderHeader orderHeader, OrderHeader updatedHeader)
        {
            orderHeader.TrackingNumber = updatedHeader.TrackingNumber;
            orderHeader.Carrier = updatedHeader.Carrier;
        }

        private void UpdateOrderStatusForShipping(OrderHeader orderHeader)
        {
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;
        }

        private void UpdatePaymentDueDateIfNeeded(OrderHeader orderHeader)
        {
            if (orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
        }

        private OrderHeader? ProcessOrderCancellation(OrderVM orderVM)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderVM.orderHeader.Id);
            if (orderHeader == null)
            {
                return null;
            }

            CancelOrder(orderHeader);
            return orderHeader;
        }

        private void CancelOrder(OrderHeader orderHeader)
        {
            bool isApprovedPayment = orderHeader.PaymentStatus == SD.PaymentStatusApproved &&
                                     orderHeader.PaymentIntentId != null;

            if (isApprovedPayment)
            {
                ProcessRefund(orderHeader.PaymentIntentId);
                _orderHeaderService.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
                return;
            }

            _orderHeaderService.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
        }

        private void ProcessRefund(string paymentIntentId)
        {
            var options = new RefundCreateOptions
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = paymentIntentId
            };

            new RefundService().Create(options);
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
            var options = CreateSessionOptions(orderVM, domain);
            AddLineItemsToSession(options, orderVM.orderDetails);

            var session = new SessionService().Create(options);
            _orderHeaderService.UpdateStripePaymentID(orderVM.orderHeader.Id, session.Id, session.PaymentIntentId);
            return session.Url;
        }

        private SessionCreateOptions CreateSessionOptions(OrderVM orderVM, string domain)
        {
            return new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderVM.orderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?id={orderVM.orderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
        }

        private void AddLineItemsToSession(SessionCreateOptions options, IEnumerable<OrderDetail> orderDetails)
        {
            foreach (var item in orderDetails)
            {
                if (item.Product == null)
                {
                    continue;
                }

                options.LineItems.Add(CreateLineItem(item));
            }
        }

        private SessionLineItemOptions CreateLineItem(OrderDetail item)
        {
            return new SessionLineItemOptions
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
            };
        }

        private OrderHeader? ProcessPaymentConfirmation(int orderHeaderId)
        {
            var orderHeader = _orderHeaderService.GetOrderHeaderById(orderHeaderId);
            if (orderHeader == null)
            {
                return null;
            }

            UpdatePaymentStatusIfNeeded(orderHeader);
            return orderHeader;
        }

        private void UpdatePaymentStatusIfNeeded(OrderHeader orderHeader)
        {
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                return;
            }

            var session = new SessionService().Get(orderHeader.SessionId);
            if (!IsPaymentComplete(session))
            {
                return;
            }

            UpdatePaymentStatus(orderHeader, session);
        }

        private bool IsPaymentComplete(Session session)
        {
            return string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase);
        }

        private void UpdatePaymentStatus(OrderHeader orderHeader, Session session)
        {
            _orderHeaderService.UpdateStripePaymentID(orderHeader.Id, session.Id, session.PaymentIntentId);

            string orderStatus = orderHeader.OrderStatus ?? SD.StatusShipped;
            _orderHeaderService.UpdateStatus(orderHeader.Id, orderStatus, SD.PaymentStatusApproved);
        }

        #endregion
    }
}