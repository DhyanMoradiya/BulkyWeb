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
                //instert
                ProductVM productVM = new()
                {
                    Product = new Product(),
                    CategoryList = CategoryList
                };
                return View(productVM);
            }
            else {
                Product product = _unitOfWork.ProductRepository.Get(u => u.Id == Id, includeProperties: "ProductImages");
                ProductVM productVM = new()
                {
                    Product = product,
                    CategoryList = CategoryList
                };
                return View(productVM);
            }
           
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, List<IFormFile> imageFiles)
        {
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;

                if (productVM.Product.Id == 0 || productVM.Product.Id == null)
                {
                    _unitOfWork.ProductRepository.Add(productVM.Product);

                }
                else
                {
                    _unitOfWork.ProductRepository.Update(productVM.Product);
                }
                _unitOfWork.Save();


                if (imageFiles != null)
                {
                    foreach (IFormFile imageFile in imageFiles)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string productpath = @"images\products\product-" + productVM.Product.Id;
                        string finalPath = Path.Combine(wwwRootPath,productpath);

                        if (!Directory.Exists(finalPath))
                        {
                            Directory.CreateDirectory(finalPath);
                        }

                        using (var fileStream = new FileStream(Path.Combine(finalPath, fileName), FileMode.Create))
                        {
                            imageFile.CopyTo(fileStream);
                        }

                        ProductImage productImage = new()
                        {
                            ImageURL = @"\" + productpath + @"\" + fileName,
                            ProductId = productVM.Product.Id,
                        };

                        if (productVM.Product.ProductImages == null)
                            productVM.Product.ProductImages = new List<ProductImage>();


                        productVM.Product.ProductImages.Add(productImage);
                    }

                    _unitOfWork.ProductRepository.Update(productVM.Product);
                    _unitOfWork.Save();

                }
                    TempData["success"] = "PRODUCT CREATED/UPDATED SUCCESSFULLY";
                return RedirectToAction("Index");
            }
            return View(productVM);
        }

        public IActionResult DeleteImage(int imageId)
        {
            ProductImage imageToBeDeleted = _unitOfWork.ProductImageRepository.Get(u => u.Id == imageId);
            int productId = imageToBeDeleted.ProductId;
            if(imageToBeDeleted != null)
            {
                if (!(string.IsNullOrEmpty(imageToBeDeleted.ImageURL)))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, imageToBeDeleted.ImageURL.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.ProductImageRepository.Remove(imageToBeDeleted);
                _unitOfWork.Save();
                TempData["success"] = "IMAGE DELETED SUCCESSFULLY";
            }
           
            return RedirectToAction(nameof(Upsert), new { id = productId });
        }


        #region API CALSS

        [HttpGet]
        public IActionResult GetAllProducts()
        {
            List<Product> productList = _unitOfWork.ProductRepository.GetAll(null, "Category").ToList();
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

                string productpath = @"images\products\product-" + id;
                string finalPath = Path.Combine(_webHostEnvironment.WebRootPath, productpath);

                if (Directory.Exists(finalPath))
                {
                    string[] filepaths = Directory.GetFiles(finalPath);
                    foreach (string filepath in filepaths)
                    {
                        System.IO.File.Delete(filepath);
                    }
                    Directory.Delete(finalPath);
                }

                _unitOfWork.ProductRepository.Remove(productTobeDeleted);
                _unitOfWork.Save();
                TempData["success"] = "PRODUCT DELETED SUCCESSFULLY";
                return Json(new { success = true, message = "Delete successfully" });
            }
            return Json(new { success = false, message = "Delete Fail !" });
        }
        

        #endregion

    }
}
