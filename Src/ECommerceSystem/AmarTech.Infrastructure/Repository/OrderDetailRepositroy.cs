using AmarTech.Domain.Entities;
using AmarTech.Infrastructure.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AmarTech.Infrastructure.Repository
{
    public class OrderDetailRepositroy : Repository<OrderDetail>, IOrderDetailRepository
    {
        private readonly ApplicationDbContext _db;
        public OrderDetailRepositroy(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderDetail obj)
        {
            _db.OrderDetails.Update(obj);
        }
    }
}
