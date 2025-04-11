using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerceSystem.DataAccess.Repository;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;

namespace ECommerceSystem.Service.Services
{
    public class OrderHeaderService : IOrderHeaderService
    {
        private readonly IOrderHeaderRepository orderHeaderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderHeaderService(IOrderHeaderRepository orderHeaderRepository,IUnitOfWork unitOfWork)
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

        public IEnumerable<OrderHeader> GetAllOrderHeaders()
        {
            return orderHeaderRepository.GetAll();
        }

        public OrderHeader? GetOrderHeaderById(int? id,string? includeProperty=null )
        {
            if(includeProperty != null)
            {
                return orderHeaderRepository.Get(u=>u.Id == id,includeProperty);
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
    }
}
