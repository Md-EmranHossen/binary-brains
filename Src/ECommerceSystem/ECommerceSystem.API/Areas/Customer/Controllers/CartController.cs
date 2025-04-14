using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services;
using ECommerceSystem.Service.Services.IServices;
using ECommerceWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace ECommerceWebApp.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IOrderHeaderService _orderHeaderService;
        private readonly IOrderDetailService _orderDetailService;
        public CartController(IShoppingCartService shoppingCartService, IUnitOfWork unitOfWork, IOrderHeaderService orderHeaderService, IApplicationUserService applicationUserService, IOrderDetailService orderDetailService)
        {
            _shoppingCartService = shoppingCartService;
            _unitOfWork = unitOfWork;
            _orderHeaderService = orderHeaderService;
            _applicationUserService = applicationUserService;
            _orderDetailService = orderDetailService;

        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var shoppingCartVM = _shoppingCartService.GetShoppingCartVM(userId);


            return View(shoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;

            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var shoppingCartVM = _shoppingCartService.GetShoppingCartVM(userId);

            shoppingCartVM.OrderHeader.ApplicationUser = _applicationUserService.GetUserById(userId);



            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            return View(shoppingCartVM);
        }
        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost(ShoppingCartVM shoppingCartVM)
        {
            /*if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }*/
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var shoppingCartList = _shoppingCartService.GetShoppingCartsByUserId(userId) ?? new List<ShoppingCart>();

            var applicationUser = _applicationUserService.GetUserById(userId);

            shoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartList = shoppingCartList,
                OrderHeader = new OrderHeader
                {
                    ApplicationUserId = userId,
                    OrderDate = DateTime.Now,
                    OrderTotal = (double)shoppingCartList
                                    .Where(cart => cart.Product != null)
                                    .Sum(cart => cart.Product.Price * cart.Count),

                    // ✅ Fill required fields from ApplicationUser
                    Name = applicationUser.Name,
                    PhoneNumber = applicationUser.PhoneNumber,
                    StreetAddress = applicationUser.StreetAddress,
                    City = applicationUser.City,
                    State = applicationUser.State,
                    PostalCode = applicationUser.PostalCode
                }
            };

            // Set payment & order status
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
            }
            else
            {
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
            }

            // Save OrderHeader
            _orderHeaderService.AddOrderHeader(shoppingCartVM.OrderHeader);
            _unitOfWork.Commit(); // commit early to generate OrderHeader.Id

            // Save OrderDetails
            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _orderDetailService.AddOrderDetail(orderDetail);
            }
            _unitOfWork.Commit();

            // Stripe Checkout for individual users
            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7000/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (var item in shoppingCartVM.ShoppingCartList)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(20 * 100), // cents
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
                var session = service.Create(options);

                _orderHeaderService.UpdateStripePaymentID(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Commit();

                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return RedirectToAction(nameof(OrderConfirmation), new { id = shoppingCartVM.OrderHeader.Id });
        }


        public IActionResult OrderConfirmation(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var orderHeader = _orderHeaderService.OrderConfirmation(id);

            _shoppingCartService.RemoveShoppingCarts(orderHeader);

            return View(id);
        }

        public IActionResult Plus(int cartId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _shoppingCartService.Plus(cartId);

            return RedirectToAction(nameof(Index));

        }
        public IActionResult Minus(int cartId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _shoppingCartService.Minus(cartId);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var cartFromDb = _shoppingCartService.GetShoppingCartById(cartId, true);
            _shoppingCartService.RemoveCartValue(cartId);
            HttpContext.Session.SetInt32(SD.SessionCart, _shoppingCartService.
GetShoppingCartByUserId(cartFromDb.ApplicationUserId).Count() - 1);

            return RedirectToAction(nameof(Index));
        }

    }
}



