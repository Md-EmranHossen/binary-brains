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

        IEnumerable<OrderHeader> GetAllOrderHeaders(string? includeProperties = null);
        OrderHeader? GetOrderHeaderById(int? id, string? includeProperty = null);
        void AddOrderHeader(OrderHeader orderHeader);
        void UpdateOrderHeader(OrderHeader orderHeader);
        void DeleteOrderHeader(int? id);
        void UpdateStatus(int id, string orderStatus, string? paymentStatus = null);
        void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId);

        OrderHeader? OrderConfirmation(int id);
    }
}
