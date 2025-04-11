﻿using System;
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
        private readonly IUnitOfWork _unitOfWork;

        public OrderDetailService(IOrderDetailRepository orderDetailRepository,IUnitOfWork unitOfWork)
        {
            this.orderDetailRepository = orderDetailRepository;
            _unitOfWork = unitOfWork;
        }
        public void AddOrderDetail(OrderDetail orderDetail)
        {
            orderDetailRepository.Add(orderDetail);
            _unitOfWork.Commit();

        }

        public void DeleteOrderDetail(int? id)
        {
            if (id != null)
            {
                var order = GetOrderDetailById(id);
                if (order != null)
                {
                    orderDetailRepository.Remove(order);
                    _unitOfWork.Commit();
                }
            }
        }

        public IEnumerable<OrderDetail> GetAllOrderDetails()
        {
            return orderDetailRepository.GetAll();
        }

        public OrderDetail? GetOrderDetailById(int? id)
        {
            return orderDetailRepository.Get(u => u.Id == id);
        }

        public void UpdateOrderDetail(OrderDetail orderDetail)
        {
            orderDetailRepository.Update(orderDetail);
            _unitOfWork.Commit();
        }
    }
}
