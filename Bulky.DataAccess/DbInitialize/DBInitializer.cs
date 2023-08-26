using Bulky.Model.Models;
using Bulky.Utility;
using BulkyWeb.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.DbInitialize
{
    public class DBInitializer : IDbinitializer
    {
        private readonly ApplicationDbContext _db;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public DBInitializer(ApplicationDbContext db, RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _roleManager = roleManager;
                _userManager = userManager;
        }

        public void Initialize()
        {
            //Add Migration if panding
            try
            {
                if(_db.Database.GetPendingMigrations().Count() > 0) { 
                    _db.Database.Migrate(); 
                }
            }catch(Exception ex)
            {
                throw new Exception(ex.ToString());
            }

            //Create role of not exist
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Company)).GetAwaiter().GetResult();


                //Create Admin User
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "Admin@gmail.com",
                    Email = "Admin@gmail.com",
                    Name  = "Dhyan Moradiya",
                    PhoneNumber = "1234567890",
                    StreetAddress = "My address",
                    State = "GJ",
                    PostalCode = "1234567890",
                    City = "ABC",

                }, "Test@123").GetAwaiter().GetResult();

                ApplicationUser user  = _db.ApplicationUsers.FirstOrDefault(u => u.Email == "Admin@gmail.com");
                _userManager.AddToRoleAsync(user, SD.Role_Admin).GetAwaiter().GetResult();
            }

        }
    }
}
