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
        private const string HeaderLocation = "Location";
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
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var shoppingCartVM = _shoppingCartService.GetShoppingCartVM(userId);
            return View(shoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            if (claimsIdentity == null)
            {
                return Unauthorized();
            }

            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var shoppingCartVM = _shoppingCartService.GetShoppingCartVM(userId);
            if (shoppingCartVM == null)
            {
                return NotFound();
            }

            var user = _applicationUserService.GetUserById(userId);
            if (user == null)
            {
                return NotFound(); 
            }

            shoppingCartVM.OrderHeader.ApplicationUser = user;

            foreach (var i in shoppingCartVM.ShoppingCartList)
            {
                if (i.Product != null)
                {
                    i.Price = (double)i.Product.Price;
                }
            }

            var header = shoppingCartVM.OrderHeader;
            var appUser = header.ApplicationUser;

            if (appUser != null)
            {
                header.Name = appUser.Name ?? string.Empty;
                header.PhoneNumber = appUser.PhoneNumber ?? string.Empty;
                header.StreetAddress = appUser.StreetAddress ?? string.Empty;
                header.City = appUser.City ?? string.Empty;
                header.State = appUser.State ?? string.Empty;
                header.PostalCode = appUser.PostalCode ?? string.Empty;
            }

            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost(ShoppingCartVM shoppingCartVM)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var shoppingCartList = _shoppingCartService.GetShoppingCartsByUserId(userId) ?? new List<ShoppingCart>();
            var applicationUser = _applicationUserService.GetUserById(userId);

            if (applicationUser == null)
            {
                return NotFound("User not found.");
            }

            double orderTotal = shoppingCartList
                .Where(cart => cart.Product != null)
                .Sum(cart => (double)cart.Product.Price * cart.Count);

            shoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartList = shoppingCartList,
                OrderHeader = new OrderHeader
                {
                    ApplicationUserId = userId,
                    OrderDate = DateTime.Now,
                    OrderTotal = orderTotal,
                    Name = applicationUser.Name,
                    PhoneNumber = applicationUser.PhoneNumber??string.Empty,
                    StreetAddress = applicationUser.StreetAddress??string.Empty,
                    City = applicationUser.City??string.Empty,
                    State = applicationUser.State??string.Empty,
                    PostalCode = applicationUser.PostalCode??string.Empty
                }
            };

            foreach (var item in shoppingCartVM.ShoppingCartList)
            {
                if (item.Product != null)
                {
                    item.Price = (double)item.Product.Price;
                }
            }

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

            _orderHeaderService.AddOrderHeader(shoppingCartVM.OrderHeader);
            _unitOfWork.Commit();

            foreach (var cart in shoppingCartVM.ShoppingCartList)
            {
                if (cart.Product == null) continue;

                var orderDetail = new OrderDetail
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = (double)cart.Product.Price,
                    Count = cart.Count
                };
                _orderDetailService.AddOrderDetail(orderDetail);
            }
            _unitOfWork.Commit();

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var domain = "https://localhost:7000/";
                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={shoppingCartVM.OrderHeader.Id}",
                    CancelUrl = domain + "customer/cart/index",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment"
                };

                foreach (var item in shoppingCartVM.ShoppingCartList)
                {
                    if (item.Product == null) continue;

                    var sessionLineItem = new SessionLineItemOptions
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
                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                var session = service.Create(options);

                _orderHeaderService.UpdateStripePaymentID(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Commit();

                Response.Headers[HeaderLocation] = session.Url;

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
            _shoppingCartService.RemoveCartValue(cartId);


            return RedirectToAction(nameof(Index));
        }

    }
}



