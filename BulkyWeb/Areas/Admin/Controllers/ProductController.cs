using Bulky.DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;
using Bulky.Model.Models;
using Bulky.DataAccess.Repository.IRepositoy;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.Model.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnviroment)
        {
            _unitOfWork = unitOfWork;   
            _webHostEnvironment = webHostEnviroment;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult Upsert(int? Id)
        {
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.CategoryRepository.GetAll().ToList().Select(u =>
                new SelectListItem { Text = u.Name, Value = u.Id.ToString() });

            //ViewBag.CategoryList = CategoryList;
            // ViewData["CategoryList"] = CategoryList;

            if (Id == null || Id == 0)
            {
                //inster
                ProductVM productVM = new()
                {
                    product = new Product(),
                    categoryList = CategoryList
                };
                return View(productVM);
            }
            else {
                Product product = _unitOfWork.ProductRepository.Get(u => u.Id == Id);
                ProductVM productVM = new()
                {
                    product = product,
                    categoryList = CategoryList
                };
                return View(productVM);
            }
           
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? imageFile)
        {
            if (ModelState.IsValid) { 
            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (imageFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string productpath = Path.Combine(wwwRootPath, @"images\product");

                if (!(string.IsNullOrEmpty(productVM.product.ImageURL)))
                {
                    var oldImagePath = Path.Combine(wwwRootPath, productVM.product.ImageURL.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                using (var fileStream = new FileStream(Path.Combine(productpath, fileName), FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }
                productVM.product.ImageURL = @"\images\product\" + fileName;

            }
            if (productVM.product.Id == 0 || productVM.product.Id == null)
            {
                _unitOfWork.ProductRepository.Add(productVM.product);
                    TempData["success"] = "PRODUCT CREATED SUCCESSFULLY";
                }
            else
            {
                _unitOfWork.ProductRepository.Update(productVM.product);
                    TempData["success"] = "PRODUCT UPDATED SUCCESSFULLY";
                }
            _unitOfWork.Save();
            return RedirectToAction("Index");
            }
            else
            {
                productVM.categoryList = _unitOfWork.CategoryRepository.GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
                return View(productVM);
            }

        }


        #region API CALSS

        [HttpGet]
        public IActionResult GetAllProducts()
        {
            List<Product> productList = _unitOfWork.ProductRepository.GetAll("Category").ToList();
            return Json(new { data = productList });
        }


        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            if(id != null)
            {
                Product productTobeDeleted = _unitOfWork.ProductRepository.Get(u => u.Id == id);
                if(productTobeDeleted == null)
                {
                    return Json(new {success = false, message = "Delete Fail !"});
                }
                if (!(string.IsNullOrEmpty(productTobeDeleted.ImageURL)))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productTobeDeleted.ImageURL.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }
                _unitOfWork.ProductRepository.Remove(productTobeDeleted); 
                _unitOfWork.Save();
                TempData["success"] = "CATEGORY DELETED SUCCESSFULLY";
                return Json(new { success = true, message = "Delete successfully" });
            }
            return Json(new { success = false, message = "Delete Fail !"}); 
        }
        

        #endregion

    }
}
