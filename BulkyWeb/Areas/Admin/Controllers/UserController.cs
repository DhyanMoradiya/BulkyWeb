using Bulky.DataAccess.Repository.IRepositoy;
using Bulky.Model.Models;
using Bulky.Model.ViewModels;
using Bulky.Utility;
using BulkyWeb.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Octokit;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult RoleManagement(string userId)
        {

            RoleManagementVM roleManagementVM = new()
            {
                ApplicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId, includeProperties:"Company"),
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {   
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList  = _unitOfWork.CompanyRepository.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                })
            };

            roleManagementVM.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUserRepository.Get(u=>u.Id == userId)).GetAwaiter().GetResult().FirstOrDefault();

            return View(roleManagementVM);
        }


        [HttpPost]
        public IActionResult RoleManagement(RoleManagementVM roleManagementVM)
        {

            string oldRole = _userManager.GetRolesAsync(_unitOfWork.ApplicationUserRepository.Get(u => u.Id == roleManagementVM.ApplicationUser.Id)).GetAwaiter().GetResult().FirstOrDefault();

            ApplicationUser applicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == roleManagementVM.ApplicationUser.Id);
            if (roleManagementVM.ApplicationUser.Role != oldRole)
            {
                //Role is updated
               
                if(roleManagementVM.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                }
                if(oldRole == SD.Role_Company){
                    applicationUser.CompanyId = null;
                }
                _unitOfWork.ApplicationUserRepository.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, roleManagementVM.ApplicationUser.Role).GetAwaiter().GetResult() ;
            }
            else
            {
                if(oldRole == SD.Role_Company || roleManagementVM.ApplicationUser.CompanyId != applicationUser.CompanyId) {
                    applicationUser.CompanyId = roleManagementVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUserRepository.Update(applicationUser);
                    _unitOfWork.Save();
                }
            }

            TempData["Success"] = "Role Updated.";

            return RedirectToAction("Index");
        }




        #region API

        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> userList = _unitOfWork.ApplicationUserRepository.GetAll(includeProperties: "Company").ToList();

            foreach (var user in userList)
            {
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();

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
            var userObj = _unitOfWork.ApplicationUserRepository.Get(u=> u.Id == id);
            if (userObj == null)
            {
                return Json(new { success = false, message = "Error while locking/unlocking" });
            }
           if(userObj.LockoutEnd > DateTime.Now)
            {
                userObj.LockoutEnd = DateTime.Now;
            }
            else
            {
                userObj.LockoutEnd = DateTime.Now.AddYears(1000);
            }

           _unitOfWork.ApplicationUserRepository.Update(userObj);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Opration successfully" });
        }


        #endregion
    }

}
