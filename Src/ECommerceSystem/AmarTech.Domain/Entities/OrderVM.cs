using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace AmarTech.Domain.Entities
{ 

    [ValidateNever]
    public class OrderVM
    {
        public OrderHeader orderHeader { get; set; } = new OrderHeader();
        public IEnumerable<OrderDetail> orderDetails { get; set; } = new List<OrderDetail>();
    }
}
