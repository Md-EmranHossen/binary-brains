using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ECommerceSystem.Service.Services.IServices;
using ECommerceSystem.DataAccess;
using Microsoft.EntityFrameworkCore;
using ECommerceSystem.DataAccess.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ECommerceWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly ICompanyService _companyService;

        public UserController(IApplicationUserService applicationUserService,ICompanyService companyService)
        {
            _applicationUserService = applicationUserService;
            _companyService = companyService;
        }

        public IActionResult Index()
        {
            var userList = _applicationUserService.GetAllUsers();

            foreach (var user in userList)
            {
                user.Role=_applicationUserService.GetUserrole(user.Id);
            }
            return View(userList);

        }

        public IActionResult LockUnlock(string? userId)
        {
            var userObj = _applicationUserService.GetUserById(userId);

            if (userObj == null)
            {
                return NotFound();
            }

            if(userObj.LockoutEnd!=null && userObj.LockoutEnd > DateTime.Now)
            {
                userObj.LockoutEnd = DateTime.Now;
            }
            else
            {
                userObj.LockoutEnd = DateTime.Now.AddDays(10);
            }
            _applicationUserService.UpdateUser(userObj);
  


            return RedirectToAction(nameof(Index));

        }

        public IActionResult RoleManagement(string? userId)
        {
            if (userId == null)
            {
                return NotFound();
            }

            RoleManagemantVM RoleVM = new RoleManagemantVM()
            {
                User = _applicationUserService.GetUserByIdAndIncludeprop(userId, "Company"),
                RoleList = _applicationUserService.GetAllRoles(),
                CompanyList = _companyService.GetAllCompanies().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };
            RoleVM.User.Role=_applicationUserService.GetUserrole(userId);

            return View(RoleVM);


        }




    }
}
