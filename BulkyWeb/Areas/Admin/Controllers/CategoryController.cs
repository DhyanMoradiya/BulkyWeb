using Bulky.DataAccess.Repository.IRepositoy;
using Bulky.Utility;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CategoryController : Controller
    {

        private readonly IUnitOfWork _unitOfWork;

        [ActivatorUtilitiesConstructor]
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<Category> categoryList = _unitOfWork.CategoryRepository.GetAll().ToList();
            return View(categoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "Category Name cant be same as Display Order");
            }
            if (!category.Name.IsNullOrEmpty() && category.Name.ToLower() == "test")
            {
                ModelState.AddModelError("", "Category Name can not be test");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.CategoryRepository.Add(category);
                _unitOfWork.Save();
                TempData["success"] = "CATEGORY CREATED SUCCESSFULLY";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }


            Category category = _unitOfWork.CategoryRepository.Get(u => u.Id == Id);
            //Category category = _db.Categories.FirstOrDefault(u => u.Id == Id);
            //Category category = _db.Categories.Where(u => u.Id == Id).FirstOrDefault();
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (category.Name == category.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "Category Name cant be same as Display Order");
            }
            if (!category.Name.IsNullOrEmpty() && category.Name.ToLower() == "test")
            {
                ModelState.AddModelError("", "Category Name can not be test");
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.CategoryRepository.Update(category);
                _unitOfWork.Save();
                TempData["success"] = "CATEGORY UPDATED SUCCESSFULLY";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            Category category = _unitOfWork.CategoryRepository.Get(u => u.Id == Id);


            if (category == null)
            {
                return NotFound();
            }

            return View(category);

        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }

            Category category = _unitOfWork.CategoryRepository.Get(u => u.Id == Id);

            if (category == null)
            {
                return NotFound();
            }

            _unitOfWork.CategoryRepository.Remove(category);
            _unitOfWork.Save();
            TempData["success"] = "CATEGORY DELETED SUCCESSFULLY";
            return RedirectToAction("Index");
        }
    }
}
