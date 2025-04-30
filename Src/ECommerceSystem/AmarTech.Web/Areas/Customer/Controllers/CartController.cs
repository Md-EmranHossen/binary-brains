﻿using AmarTech.Domain.Entities;
using AmarTech.Infrastructure.Repository.IRepository;
using AmarTech.Application.Services;
using AmarTech.Application.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;


namespace AmarTech.Web.Areas.Customer.Controllers
{
    [Area("customer")]
    public class CartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;

        private readonly IApplicationUserService _applicationUserService;
        private readonly IOrderHeaderService _orderHeaderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly IProductService _productService;
        private const string HeaderLocation = "Location";
        public CartController(IShoppingCartService shoppingCartService, IOrderHeaderService orderHeaderService, IApplicationUserService applicationUserService, IOrderDetailService orderDetailService, IProductService productService)
        {
            _shoppingCartService = shoppingCartService;
            _orderHeaderService = orderHeaderService;
            _applicationUserService = applicationUserService;
            _orderDetailService = orderDetailService;
            _productService = productService;
        }

        public IActionResult Index()
        {
            var claimsIdentity = User.Identity as ClaimsIdentity;
            var userId = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ShoppingCartVM shoppingCartVM;
            if (userId == null)
            {
                var shoppingCartList = GetMemoryShoppingCartList();

                shoppingCartVM = _shoppingCartService.MemoryCartVM(shoppingCartList);

            }
            else
            {
                shoppingCartVM = _shoppingCartService.GetShoppingCartVM(userId);
            }
            return View(shoppingCartVM);
        }
        [Authorize]
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
            ShoppingCartVM shoppingCartVM;
            var cartList = _shoppingCartService.GetCart();
            var shoppingCartList = _shoppingCartService.GetShoppingCartsByUserId(userId).ToList();

            if (cartList.Count > 0)
            {
                shoppingCartVM = _shoppingCartService.CombineToDB(shoppingCartList, cartList, userId);
            }
            else
            {
                shoppingCartVM = _shoppingCartService.GetShoppingCartVM(userId);
            }


            if (shoppingCartVM == null)
            {
                return NotFound();
            }
            var shoppingCartCount = _shoppingCartService.GetShoppingCartByUserId(userId)?.Count() ?? 0;
            HttpContext.Session.SetInt32(SD.SessionCart, shoppingCartCount);

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
                    i.Price = (double)(i.Product.Price - i.Product.DiscountAmount);
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
            if (!ModelState.IsValid)//need
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

            shoppingCartVM = _shoppingCartService.GetShoppingCartVMForSummaryPost(shoppingCartList, applicationUser, userId);

            _orderHeaderService.AddOrderHeader(shoppingCartVM.OrderHeader);

            _orderDetailService.UpdateOrderDetailsValues(shoppingCartVM);

            if (applicationUser.CompanyId.GetValueOrDefault() == 0)
            {
                var options = _shoppingCartService.CheckOutForUser(shoppingCartVM);

                var service = new SessionService();
                var session = service.Create(options);

                _orderHeaderService.UpdateStripePaymentID(shoppingCartVM.OrderHeader.Id, session.Id, session.PaymentIntentId);

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



            var cartList = _shoppingCartService.RemoveShoppingCarts(orderHeader);
            _productService.ReduceStockCount(cartList);


            return View(id);
        }

        public IActionResult Plus(int cartId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var cartFromDb = _shoppingCartService.GetShoppingCartById(cartId);
            if (cartFromDb != null)
            {
                cartFromDb.Product = _productService.GetProductById(cartFromDb.ProductId) ?? new Product();
            }
            _shoppingCartService.Plus(cartFromDb, cartId);

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

        List<ShoppingCart> GetMemoryShoppingCartList()
        {
            var shoppingCartList = _shoppingCartService.GetCart();

            foreach (var cart in shoppingCartList)
            {
                if (cart == null)
                    continue;

                var product = _productService.GetProductById(cart.ProductId);
                if (product != null)
                {
                    cart.Product = product;
                }
            }
            return shoppingCartList;
        }

    }
}