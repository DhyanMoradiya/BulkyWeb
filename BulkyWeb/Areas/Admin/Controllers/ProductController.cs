using Bulky.DataAccess.Repository;
using Microsoft.AspNetCore.Mvc;
using Bulky.Model.Models;
using Bulky.DataAccess.Repository.IRepositoy;
using Microsoft.AspNetCore.Mvc.Rendering;
using Bulky.Model.ViewModels;

namespace BulkyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
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
            List<Product> productList = _unitOfWork.ProductRepository.GetAll().ToList();
            return View(productList);
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

            string wwwRootPath = _webHostEnvironment.WebRootPath;
            if (imageFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string productpath = Path.Combine(wwwRootPath, @"images\product");

                using (var fileStream = new FileStream(Path.Combine(productpath, fileName), FileMode.Create))
                {
                    imageFile.CopyTo(fileStream);
                }
                productVM.product.ImageURL = @"\images\product\" + fileName;
                _unitOfWork.ProductRepository.Add(productVM.product);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }else { 
                productVM.categoryList = _unitOfWork.CategoryRepository.GetAll().Select(u => new SelectListItem { 
                Text = u.Name,
                Value = u.Id.ToString()
                });
                return View(productVM);
            }
           
        }

        public IActionResult Delete(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            Product product = _unitOfWork.ProductRepository.Get(u => u.Id == Id);
            IEnumerable<SelectListItem> CategoryList = _unitOfWork.CategoryRepository.GetAll().ToList().Select(u =>
            new SelectListItem { Text = u.Name, Value = u.Id.ToString() });

            ProductVM productVM = new()
            {
                product = product,
                categoryList = CategoryList
            };
            return View(productVM);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? Id)
        {
            if (Id == null)
            {
                return NotFound();
            }
            Product product = _unitOfWork.ProductRepository.Get(u => u.Id == Id);
            _unitOfWork.ProductRepository.Remove(product);
            _unitOfWork.Save();
            return RedirectToAction("index");
        }

    }
}
