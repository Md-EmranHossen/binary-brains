using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using ECommerceSystem.Service.Services.IServices;

namespace ECommerceWebApp.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IApplicationUserRepository _applicationUserRepositroy;
        private readonly IUnitOfWork _unitOfWork;

        public ApplicationUserService(IApplicationUserRepository ApplicationUserRepositroy,IUnitOfWork unitOfWork)
        {
            _applicationUserRepositroy = ApplicationUserRepositroy;
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<ApplicationUser> GetAllUsers()
        {
            var obj= _applicationUserRepositroy.GetAll(includeProperties: "Company");
            return obj;
        }

        public string GetUserrole(string userId)
        {
           return _applicationUserRepositroy.GetUserRole(userId);
        }

        public void UpdateUser(ApplicationUser user)
        {
            _applicationUserRepositroy.Update(user);
            _unitOfWork.Commit();

        }

        ApplicationUser? IApplicationUserService.GetUserById(string? id)
        {
            return _applicationUserRepositroy.Get(u => u.Id == id);
        }

    }
}
