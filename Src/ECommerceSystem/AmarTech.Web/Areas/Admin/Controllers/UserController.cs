using AmarTech.Infrastructure.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AmarTech.Application.Services.IServices;
using AmarTech.Infrastructure;
using Microsoft.EntityFrameworkCore;
using AmarTech.Infrastructure.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using AmarTech.Domain.Entities;


namespace AmarTech.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly ICompanyService _companyService;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(IApplicationUserService applicationUserService,ICompanyService companyService, UserManager<IdentityUser> userManager)
        {
            _applicationUserService = applicationUserService;
            _companyService = companyService;
            _userManager = userManager;
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
            if (string.IsNullOrEmpty(userId))
            {
                return NotFound();
            }

            var user = _applicationUserService.GetUserByIdAndIncludeprop(userId, "Company");
            if (user == null)
            {
                return NotFound(); // or show a user-friendly error page
            }

            RoleManagemantVM RoleVM = new RoleManagemantVM()
            {
                User = user,
                RoleList = _applicationUserService.GetAllRoles(),
                CompanyList = _companyService.GetAllCompanies().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString()
                }),
            };

            RoleVM.User.Role = _applicationUserService.GetUserrole(userId);

            return View(RoleVM);
        }


        [HttpPost]
        public IActionResult RoleManagement(RoleManagemantVM roleManagmentVM)
        {
            if (!ModelState.IsValid)//need 
            {
                return View(roleManagmentVM);
            }

            ApplicationUser? applicationUser = _applicationUserService.GetUserById(roleManagmentVM.User?.Id);
            if (applicationUser == null)
            {
                return NotFound("User not found");
            }

            var oldRole = _userManager.GetRolesAsync(applicationUser)
                .GetAwaiter().GetResult().FirstOrDefault() ?? string.Empty;

            if (
     !string.IsNullOrEmpty(roleManagmentVM.User?.Role) &&
     !string.Equals(roleManagmentVM.User.Role, oldRole, StringComparison.OrdinalIgnoreCase))
            {
                // A role was updated

                if (roleManagmentVM.User.Role == SD.Role_Company)
                    {
                        applicationUser.CompanyId = roleManagmentVM.User.CompanyId;
                    }
                    if (oldRole == SD.Role_Company)
                    {
                        applicationUser.CompanyId = null;
                    }
                    _applicationUserService.UpdateUser(applicationUser);

                    if (!string.IsNullOrEmpty(oldRole))
                    {
                        _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                    }
                    _userManager.AddToRoleAsync(applicationUser, roleManagmentVM.User.Role).GetAwaiter().GetResult();
                
            }

            return RedirectToAction("Index");
        }




    }
}
