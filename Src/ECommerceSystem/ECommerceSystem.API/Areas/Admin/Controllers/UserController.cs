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
            return View(userList);

        }



    
    }
}
