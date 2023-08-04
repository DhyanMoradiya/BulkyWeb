using Bulky.DataAccess.Repository.IRepositoy;
using Bulky.Model.Models;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Company> companyList =  _unitOfWork.CompanyRepository.GetAll().ToList();
            return View(companyList);
        }

        public IActionResult Upsert(int? Id) {
            if (Id != null || Id != 0) { 
                Company company = _unitOfWork.CompanyRepository.Get(u => u.Id == Id);
                if (company != null)
                {
                    return View(company);
                }
            }
            return View(new Company());
        }

        [HttpPost]
        public IActionResult Upsert(Company company)
        {
            if (ModelState.IsValid)
            {
                if(company.Id == 0)
                {
                    _unitOfWork.CompanyRepository.Add(company);
                    TempData["success"] = "COMPANY CREATED SUCCESSFULLY";
                }
                else
                {
                    _unitOfWork.CompanyRepository.Update(company);
                    TempData["success"] = "COMPANY UPDATED SUCCESSFULLY";
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(company);
        }


        #region API

        [HttpGet]
        public IActionResult GetAllCompany()
        {
            List<Company> companyList = _unitOfWork.CompanyRepository.GetAll().ToList();
            return Json(new { data = companyList });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            if(id == 0 || id == null) {
                return Json(new {success = false, message = "Delete Fail !"});
            }
            Company company = _unitOfWork.CompanyRepository.Get(u => u.Id == id);
            _unitOfWork.CompanyRepository.Remove(company);
            _unitOfWork.Save();
            TempData["success"] = "COMPANY DELETED SUCCESSFULLY";
            return Json(new { success = true, message = "Deleted successfully" });
        }


        #endregion
    }

}
