using System.Linq.Expressions;

namespace ECommerceSystem.DataAccess.Repository.IRepository
{
    public interface IRepository
    {
        public interface IRepository<T> where T : class
        {
            //T - Category
            IEnumerable<T> GetAll();
            T Get(Expression<Func<T, bool>> filter);
            void Add(T entity);
            void Remove(T entity);
            void RemoveRange(IEnumerable<T> entity);
        }
    }
}
