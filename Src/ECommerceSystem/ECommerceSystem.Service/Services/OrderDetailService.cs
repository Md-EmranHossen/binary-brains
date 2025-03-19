using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;

namespace ECommerceSystem.Service.Services
{
    public class OrderDetailService : IOrderDetailService
    {
        private readonly IOrderDetailRepository orderDetailRepository;
        public OrderDetailService(IOrderDetailRepository orderDetailRepository)
        {
            this.orderDetailRepository = orderDetailRepository;
        }
        public void AddOrderDetail(OrderDetail orderDetail)
        {
            orderDetailRepository.Add(orderDetail);
        }

        public void DeleteOrderDetail(int? id)
        {
            var order = GetOrderDetailById(id);
            if (order != null)
            {
                orderDetailRepository.Remove(order);
            }
        }

        public IEnumerable<OrderDetail> GetAllOrderDetails()
        {
            return orderDetailRepository.GetAll();
        }

        public OrderDetail GetOrderDetailById(int? id)
        {
            return orderDetailRepository.Get(u => u.Id == id);
        }

        public void UpdateOrderDetail(OrderDetail orderDetail)
        {
            orderDetailRepository.Update(orderDetail);
        }
    }
}
