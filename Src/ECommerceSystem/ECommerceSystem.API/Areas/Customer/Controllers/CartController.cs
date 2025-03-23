using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services;
using ECommerceSystem.Service.Services.IServices;
using ECommerceWebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerceWebApp.Areas.Customer.Controllers
{
    [Area("customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderHeaderService _orderHeaderService;
        public CartController(IShoppingCartService shoppingCartService, IUnitOfWork unitOfWork, IOrderHeaderService orderHeaderService)
        {
            _shoppingCartService = shoppingCartService;
            _unitOfWork = unitOfWork;
            _orderHeaderService = orderHeaderService;
        }

        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var shoppingCartList = _shoppingCartService.GetShoppingCartsByUserId(userId) ?? new List<ShoppingCart>(); // Ensure not null


            var shoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartList = shoppingCartList,
                OrderHeader = new OrderHeader
                {
                    OrderTotal = (double)shoppingCartList.Where(cart => cart.Product != null) // Avoid null references
                                             .Sum(cart => cart.Product.Price * cart.Count)
                }


            };

            return View(shoppingCartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var shoppingCartList = _shoppingCartService.GetShoppingCartsByUserId(userId) ?? new List<ShoppingCart>(); // Ensure not null


            var shoppingCartVM = new ShoppingCartVM
            {
                ShoppingCartList = shoppingCartList,
                OrderHeader = new OrderHeader
                {
                    OrderTotal = (double)shoppingCartList.Where(cart => cart.Product != null) // Avoid null references
                                             .Sum(cart => cart.Product.Price * cart.Count)
                }


            };
            //shoppingCartVM.OrderHeader = _orderHeaderService.GetOrderHeaderById(userId);

            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;

            return View(shoppingCartVM);
        }

        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _shoppingCartService.GetShoppingCartById(cartId);
            if (cartFromDb == null)
            {
                return RedirectToAction("Index");
            }
            cartFromDb.Count += 1;
            _shoppingCartService.UpdateShoppingCart(cartFromDb);
            _unitOfWork.Commit();

            return RedirectToAction("Index");
        }
        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _shoppingCartService.GetShoppingCartById(cartId);
            if (cartFromDb == null)
            {
                return RedirectToAction("Index");
            }

            if (cartFromDb.Count <= 1)
            {
                _shoppingCartService.DeleteShoppingCart(cartId);
            }
            else
            {
                cartFromDb.Count -= 1;
                _shoppingCartService.UpdateShoppingCart(cartFromDb);
            }

            _unitOfWork.Commit();
            return RedirectToAction("Index");
        }

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _shoppingCartService.GetShoppingCartById(cartId);
            if (cartFromDb == null)
            {
                return RedirectToAction("Index");
            }
            _shoppingCartService.DeleteShoppingCart(cartId);
            _unitOfWork.Commit();
            return RedirectToAction("Index");
        }
    }
}



