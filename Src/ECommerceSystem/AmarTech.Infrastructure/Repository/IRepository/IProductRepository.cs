﻿using AmarTech.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AmarTech.Infrastructure.Repository.IRepository.IRepository;

namespace AmarTech.Infrastructure.Repository.IRepository
{
    public interface IProductRepository : IRepository<Product>
    {
        void Update(Product obj);
        IEnumerable<Product> SkipAndTake(int pageSize, int pageNumber, string? searchQuery = null, string? includeProperties = null);

        void ReduceStockCount(List<ShoppingCart> cartList);
        int GetAllProductsCount(string? searchQuery = null);
    }
}
