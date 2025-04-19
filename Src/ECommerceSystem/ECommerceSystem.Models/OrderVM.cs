using System.Collections.Generic;

namespace ECommerceSystem.Models
{
    public class OrderVM
    {
        public OrderHeader OrderHeader { get; set; } = new OrderHeader();
        public IEnumerable<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
