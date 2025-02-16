using ECommerceSystem.API.Data;
using ECommerceSystem.API.Models;

namespace ECommerceSystem.API.Services
{
        public class CategoryService : ICategoryService
        {
            private readonly ApplicationDbContext _db;

            public CategoryService(ApplicationDbContext db)
            {
                _db = db;
            }

            public IEnumerable<Category> GetAllCategories()
            {
                return _db.Categories.ToList();
            }

            public Category GetCategoryById(int id)
            {
                return _db.Categories.Find(id);
            }

            public void AddCategory(Category category)
            {
                _db.Categories.Add(category);
                _db.SaveChanges();
            }

            public void UpdateCategory(Category category)
            {
                _db.Categories.Update(category);
                _db.SaveChanges();
            }

            public void DeleteCategory(int id)
            {
                var category = _db.Categories.Find(id);
                if (category != null)
                {
                    _db.Categories.Remove(category);
                    _db.SaveChanges();
                }
            }
        }
    }
