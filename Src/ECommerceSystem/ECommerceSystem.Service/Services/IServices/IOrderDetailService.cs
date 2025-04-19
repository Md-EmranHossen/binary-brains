using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerceSystem.Models;

namespace ECommerceSystem.Service.Services.IServices
{
    public interface IOrderDetailService
    {

        IEnumerable<OrderDetail> GetAllOrderDetails();
        OrderDetail? GetOrderDetailById(int? id);
        void AddOrderDetail(OrderDetail orderDetail);
        void UpdateOrderDetail(OrderDetail orderDetail);
        void DeleteOrderDetail(int? id);

        IEnumerable<OrderDetail> GetAllOrders(int? id,string? includeProperties=null);
    }
}