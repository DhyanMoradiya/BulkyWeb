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
        private readonly RoleManager<IdentityUser> _roleManager;

        public UserController(IUnitOfWork unitOfWork, UserManager<IdentityUser> userManager, RoleManager<IdentityUser> roleManager)
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
                RoleList = _roleManager.Roles.Select(i => new SelectListItem{
                    Text = i.Name,
                    Value = i.Name.ToString(),
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


            if (roleManagementVM.ApplicationUser.Role != oldRole)
            {
                //Role is updated
                ApplicationUser applicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == roleManagementVM.ApplicationUser.Id);
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

            TempData["Success"] = "Role Updated.";

            return RedirectToAction("Index");
        }




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
           if(userObj.LockoutEnd > DateTime.Now)
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
