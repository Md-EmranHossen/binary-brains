using System.Collections.Generic;

namespace ECommerceSystem.Models
{
    public class ShoppingCartVM
    {
        public IEnumerable<ShoppingCart> ShoppingCartList { get; set; } = Enumerable.Empty<ShoppingCart>();
        public OrderHeader OrderHeader { get; set; } = new OrderHeader();
    }
}
