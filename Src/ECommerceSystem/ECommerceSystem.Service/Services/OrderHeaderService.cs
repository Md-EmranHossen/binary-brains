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
    public class OrderHeaderService : IOrderHeaderService
    {
        private readonly IOrderHeaderRepository orderHeaderRepository;
        public OrderHeaderService(IOrderHeaderRepository orderHeaderRepository)
        {
            this.orderHeaderRepository = orderHeaderRepository;
        }
        public void AddOrderHeader(OrderHeader orderHeader)
        {
            orderHeaderRepository.Add(orderHeader);
        }

        public void DeleteOrderHeader(int? id)
        {
            var order = GetOrderHeaderById(id);
            if (order != null)
            {
                orderHeaderRepository.Remove(order);
            }
        }

        public IEnumerable<OrderHeader> GetAllOrderHeaders()
        {
            return orderHeaderRepository.GetAll();
        }

        public OrderHeader GetOrderHeaderById(int? id)
        {
            return orderHeaderRepository.Get(u => u.Id == id);
        }

        public void UpdateOrderHeader(OrderHeader orderHeader)
        {
            orderHeaderRepository.Update(orderHeader);
        }
    }
}
