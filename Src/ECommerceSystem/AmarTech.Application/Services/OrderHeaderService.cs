using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmarTech.Domain.Entities;
using AmarTech.Infrastructure.Repository;
using AmarTech.Infrastructure.Repository.IRepository;
using AmarTech.Application.Services.IServices;
using Microsoft.AspNetCore.Http;
using Stripe.Checkout;

namespace AmarTech.Application.Services
{
    public class OrderHeaderService : IOrderHeaderService
    {
        private readonly IOrderHeaderRepository orderHeaderRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrderHeaderService(IOrderHeaderRepository orderHeaderRepository, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            this.orderHeaderRepository = orderHeaderRepository;
            _unitOfWork = unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }
        public void AddOrderHeader(OrderHeader orderHeader)
        {
            orderHeaderRepository.Add(orderHeader);
            _unitOfWork.Commit();
        }

        public void DeleteOrderHeader(int? id)
        {
            if (id != null)
            {
                var order = GetOrderHeaderById(id);
                if (order != null)
                {
                    orderHeaderRepository.Remove(order);
                    _unitOfWork.Commit();
                }
            }
        }

        public IEnumerable<OrderHeader> GetAllOrderHeaders(string? includeProperties = null)
        {

            return orderHeaderRepository.GetAll(includeProperties: includeProperties);

        }

        public OrderHeader? GetOrderHeaderById(int? id, string? includeProperty = null)
        {
            if (includeProperty != null)
            {
                return orderHeaderRepository.Get(u => u.Id == id, includeProperty);
            }
            else
            {
                return orderHeaderRepository.Get(u => u.Id == id);
            }
        }

        public void UpdateOrderHeader(OrderHeader orderHeader)
        {
            orderHeaderRepository.Update(orderHeader);
            _unitOfWork.Commit();
        }

        public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
        {
            orderHeaderRepository.UpdateStatus(id, orderStatus, paymentStatus);
            _unitOfWork.Commit();
        }

        public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
        {
            orderHeaderRepository.UpdateStripePaymentID(id, sessionId, paymentIntentId);
            _unitOfWork.Commit();
        }

        public OrderHeader? OrderConfirmation(int id)
        {
            var orderHeader = GetOrderHeaderById(id, "ApplicationUser");
            if (orderHeader != null && orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {


                var service = new SessionService();
                var session = service.Get(orderHeader.SessionId);

                if (string.Equals(session.PaymentStatus, "paid", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateStripePaymentID(id, session.Id, session.PaymentIntentId);
                    UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Commit();

                }
                _httpContextAccessor.HttpContext?.Session.Clear();

            }

            return orderHeader;

        }

        public IEnumerable<OrderHeader> GetAllOrderHeadersById(string id, string? includeProperties = null)
        {
            return orderHeaderRepository.GetAll(u => u.ApplicationUserId == id, includeProperties);
        }

        public int GetAllOrderHeadersCount()
        {
            return orderHeaderRepository.GetAllOrderHeadersCount();
        }
    }
}
