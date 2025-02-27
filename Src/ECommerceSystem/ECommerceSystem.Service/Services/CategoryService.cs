using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;

namespace ECommerceWebApp.Services
{
    public class CategoryService : ICategoryService
        {
        private readonly ICategoryRepository categoryRepositroy;

        public CategoryService(ICategoryRepository categoryRepositroy)
            {
 
            this.categoryRepositroy = categoryRepositroy;
        }

            public IEnumerable<Category> GetAllCategories()
            {
                return categoryRepositroy.GetAll();
            }

            public Category GetCategoryById(int? id)
            {
                return categoryRepositroy.Get(u => u.Id == id);
            }

            public void AddCategory(Category category)
            {
                categoryRepositroy.Add(category);
            
            }

            public void UpdateCategory(Category category)
            {
                categoryRepositroy.Update(category);
         
            }

            public void DeleteCategory(int? id)
            {
                var category = GetCategoryById(id);
                if (category != null)
                {
                    categoryRepositroy.Remove(category);
    
                }
            }
        }
    }
