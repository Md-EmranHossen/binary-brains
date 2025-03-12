using ECommerceSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceSystem.DataAccess.Repository.IRepository
{
    internal interface IShoppingCartRepository
    {
        void Update(ShoppingCart obj);
    }
}
