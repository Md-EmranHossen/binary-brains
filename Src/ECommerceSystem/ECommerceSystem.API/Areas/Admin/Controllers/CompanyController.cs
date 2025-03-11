using ECommerceSystem.DataAccess.Repository.IRepository;
using ECommerceSystem.Models;
using Microsoft.AspNetCore.Mvc;
using ECommerceSystem.Utility;
using Microsoft.AspNetCore.Authorization;
using ECommerceSystem.Service.Services.IServices;

namespace ECommerceWebApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICompanyService CompanyService;

        public CompanyController(IUnitOfWork unitOfWork,ICompanyService CompanyService)
        {
            _unitOfWork = unitOfWork;
            this.CompanyService = CompanyService;
        }

        public IActionResult Index()
        {
            IEnumerable<Company> CompanyList = CompanyService.GetAllCompanies();
            return View(CompanyList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Company obj)
        {
            if (ModelState.IsValid)
            {
                CompanyService.AddCompany(obj);
                _unitOfWork.Commit();
                TempData["success"] = "Company created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Company CompanyFromDb = CompanyService.GetCompanyById(id);
            if (CompanyFromDb == null)
            {
                return NotFound();
            }
            return View(CompanyFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Company obj)
        {
            if (ModelState.IsValid)
            {
            
                CompanyService.UpdateCompany(obj);
                _unitOfWork.Commit();
                TempData["success"] = "Company updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }
        public IActionResult Delete(int? id)
        {
            var CompanyFromDb = CompanyService.GetCompanyById(id);
            if (CompanyFromDb == null)
            {
                return NotFound();
            }
            return View(CompanyFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            CompanyService.DeleteCompany(id);
            _unitOfWork.Commit();
            TempData["success"] = "Company deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
