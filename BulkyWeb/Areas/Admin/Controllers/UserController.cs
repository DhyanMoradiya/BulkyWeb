using Bulky.DataAccess.Repository.IRepositoy;
using Bulky.Model.Models;
using Bulky.Utility;
using BulkyWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {

        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        //public IActionResult Upsert(int? Id) {
        //    if (Id != null || Id != 0) { 
        //        Company company = _unitOfWork.CompanyRepository.Get(u => u.Id == Id);
        //        if (company != null)
        //        {
        //            return View(company);
        //        }
        //    }
        //    return View(new Company());
        //}

        //[HttpPost]
        //public IActionResult Upsert(Company company)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if(company.Id == 0)
        //        {
        //            _unitOfWork.CompanyRepository.Add(company);
        //            TempData["success"] = "COMPANY CREATED SUCCESSFULLY";
        //        }
        //        else
        //        {
        //            _unitOfWork.CompanyRepository.Update(company);
        //            TempData["success"] = "COMPANY UPDATED SUCCESSFULLY";
        //        }
        //        _unitOfWork.Save();
        //        return RedirectToAction("Index");
        //    }
        //    return View(company);
        //}


        #region API

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> userList = _db.ApplicationUsers.Include("Company").ToList();

            var Roles = _db.Roles.ToList();
            var userRole = _db.UserRoles.ToList();

            foreach (var user in userList)
            {
                var roleId = userRole.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = Roles.FirstOrDefault(u => u.Id == roleId).Name;

                if(user.Company == null)
                {
                    user.Company = new(){
                        Name = ""
                    };
                }
            }

            return Json(new { data = userList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var userObj = _db.ApplicationUsers.FirstOrDefault(u=> u.Id == id);
            if (userObj == null)
            {
                return Json(new { success = false, message = "Error while locking/unlocking" });
            }
           if(userObj.LockoutEnd != null || userObj.LockoutEnd > DateTime.Now)
            {
                userObj.LockoutEnd = DateTime.Now;
            }
            else
            {
                userObj.LockoutEnd = DateTime.Now.AddYears(1000);
            }

            _db.SaveChanges();
            return Json(new { success = true, message = "Opration successfully" });
        }


        #endregion
    }

}
