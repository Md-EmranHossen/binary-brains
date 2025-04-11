using ECommerceSystem.DataAccess.Repository;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceSystem.Service.Services
{
    public class ShoppingCartService : IShoppingCartService
    {
        private readonly IShoppingCartRepository _shoppingCartRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ShoppingCartService(IShoppingCartRepository shoppingCartRepository, IUnitOfWork unitOfWork)
        {
            _shoppingCartRepository = shoppingCartRepository;
            _unitOfWork = unitOfWork;
        }

        public void AddShoppingCart(ShoppingCart shoppingCart)
        {
            _shoppingCartRepository.Add(shoppingCart);
            _unitOfWork.Commit();
        }

        public void DeleteShoppingCart(int? id)
        {
            if (id != null)
            {
                var shoppingcart = GetShoppingCartById(id);
                if (shoppingcart != null)
                {
                    _shoppingCartRepository.Remove(shoppingcart);
                    _unitOfWork.Commit();
                }

            }


        }

        public ShoppingCart? GetShoppingCartById(int? id)
        {
            return _shoppingCartRepository.Get(u => u.Id == id);
        }

        public ShoppingCart? GetShoppingCartByUserAndProduct(string userId, int productId)
        {
            return _shoppingCartRepository.Get(u => u.ApplicationUserId == userId && u.ProductId == productId);
        }

        public void UpdateShoppingCart(ShoppingCart shoppingCart)
        {
            var existingCart = _shoppingCartRepository.Get(u => u.Id == shoppingCart.Id);
            if (existingCart != null)
            {
                existingCart.Count = shoppingCart.Count;
                _shoppingCartRepository.Update(existingCart);
                _unitOfWork.Commit();
            }
        }

       public IEnumerable<ShoppingCart> GetShoppingCartsByUserId(string userId)
        {
            return _shoppingCartRepository.GetAll(
                u => u.ApplicationUserId == userId,
                includeProperties: "Product" // Ensure Product is loaded
            ) ?? new List<ShoppingCart>();
        }

        public void RemoveRange(List<ShoppingCart> shoppingCarts)
        {
            _shoppingCartRepository.RemoveRange(shoppingCarts);
            _unitOfWork.Commit();
        }

        public ShoppingCart CreateCartWithProduct(Product product)
        {
            return new ShoppingCart
            {
                Product = product,
                ProductId = product.Id,
                Count = 1
            };
        }

        public bool AddOrUpdateShoppingCart(ShoppingCart shoppingCart, string userId)
        {
            if (string.IsNullOrEmpty(userId) || shoppingCart == null || shoppingCart.ProductId == 0)
            {
                return false; 
            }

            shoppingCart.ApplicationUserId = userId;

            var cartFromDb = GetShoppingCartByUserAndProduct(userId, shoppingCart.ProductId);

            if (cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                UpdateShoppingCart(cartFromDb);
            }
            else
            {
                AddShoppingCart(shoppingCart);
            }

            _unitOfWork.Commit();
            return true;
        }




    }
}
