using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECommerceSystem.Models;

namespace ECommerceSystem.Service.Services.IServices
{
    public interface IOrderHeaderService
    {

        IEnumerable<OrderHeader> GetAllOrderHeaders();
        OrderHeader GetOrderHeaderById(int? id);
        void AddOrderHeader(OrderHeader orderHeader);
        void UpdateOrderHeader(OrderHeader orderHeader);
        void DeleteOrderHeader(int? id);
    }
}
