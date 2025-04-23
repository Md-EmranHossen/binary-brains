using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.Collections.Generic;

namespace ECommerceSystem.Models
{
    [ValidateNever]
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; } = Enumerable.Empty<ShoppingCart>();
        public OrderHeader OrderHeader { get; set; } = new OrderHeader();
    }
}
