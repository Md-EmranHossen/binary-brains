using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerceSystem.DataAccess.Repository;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;
using Stripe.Checkout;

namespace ECommerceSystem.Service.Services
{
    public class OrderHeaderService : IOrderHeaderService
    {
        private readonly IOrderHeaderRepository orderHeaderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderHeaderService(IOrderHeaderRepository orderHeaderRepository, IUnitOfWork unitOfWork)
        {
            this.orderHeaderRepository = orderHeaderRepository;
            _unitOfWork = unitOfWork;
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
            }

            return orderHeader;

        }
    }
}
