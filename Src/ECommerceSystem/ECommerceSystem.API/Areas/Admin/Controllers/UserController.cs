using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ECommerceSystem.Service.Services.IServices;
using ECommerceSystem.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace ECommerceWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly IApplicationUserService _applicationUserService;

        public UserController(IApplicationUserService applicationUserService)
        {
            _applicationUserService = applicationUserService;
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




    }
}
