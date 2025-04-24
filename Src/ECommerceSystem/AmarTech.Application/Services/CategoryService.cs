using AmarTech.Domain.Entities;
using AmarTech.Infrastructure.Repository;
using AmarTech.Infrastructure.Repository.IRepository;
using AmarTech.Application.Services.IServices;

namespace AmarTech.Web.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository categoryRepositroy;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(ICategoryRepository categoryRepositroy,IUnitOfWork unitOfWork)
        {

            this.categoryRepositroy = categoryRepositroy;
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<Category> GetAllCategories()
        {
            return categoryRepositroy.GetAll();
        }

        public Category? GetCategoryById(int? id)
        {
            return categoryRepositroy.Get(u => u.Id == id);
        }

        public void AddCategory(Category category)
        {
            categoryRepositroy.Add(category);
            _unitOfWork.Commit();

        }

        public void UpdateCategory(Category category)
        {
            categoryRepositroy.Update(category);
            _unitOfWork.Commit();


        }

        public void DeleteCategory(int? id)
        {
            if (id != null)
            {
                var category = GetCategoryById(id);
                if (category != null)
                {
                    categoryRepositroy.Remove(category);
                    _unitOfWork.Commit();

                }
            }
        }
    }
}
