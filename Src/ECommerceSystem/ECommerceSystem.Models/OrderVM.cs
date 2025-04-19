using System.Collections.Generic;

namespace ECommerceSystem.Models
{
    public class OrderVM
    {
        public OrderHeader orderHeader { get; set; } = new OrderHeader();
        public IEnumerable<OrderDetail> orderDetails { get; set; } = new List<OrderDetail>();
    }
}
