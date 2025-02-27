using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceSystem.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {


        void Commit();
    }
}
