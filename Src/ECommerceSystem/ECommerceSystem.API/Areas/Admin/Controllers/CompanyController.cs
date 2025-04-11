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
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        public IActionResult Index()
        {
            var companyList = _companyService.GetAllCompanies();
            return View(companyList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Company company)
        {
            if (!ModelState.IsValid)
            {
                return View(company);
            }

            _companyService.AddCompany(company);
            TempData["success"] = "Company created successfully";
            return RedirectToAction(nameof(Index));
        }

        private IActionResult LoadCompanyView(int? id, string viewName)
        {
            if (id is null || id == 0)
            {
                return NotFound();
            }

            var companyFromDb = _companyService.GetCompanyById(id);
            if (companyFromDb == null)
            {
                return NotFound();
            }

            return View(viewName, companyFromDb);
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return LoadCompanyView(id, "Edit");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Company company)
        {
            if (!ModelState.IsValid)
            {
                return View(company);
            }

            _companyService.UpdateCompany(company);
            TempData["success"] = "Company updated successfully";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return LoadCompanyView(id, "Delete");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int? id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id is null)
            {
                return NotFound();
            }

            _companyService.DeleteCompany(id);
            TempData["success"] = "Company deleted successfully";
            return RedirectToAction(nameof(Index));
        }
    }
}
