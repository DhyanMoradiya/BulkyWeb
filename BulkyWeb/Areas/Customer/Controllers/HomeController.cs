using Bulky.DataAccess.Repository.IRepositoy;
using Bulky.Model.Models;
using Bulky.Model.ViewModels;
using Bulky.Utility;
using BulkyWeb.Data;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(IUnitOfWork unitOfWOrk)
        {
            _unitOfWork = unitOfWOrk;
        }

        public IActionResult Index()
        {

            List<Product> productList = _unitOfWork.ProductRepository.GetAll(includeProperties: "Category, ProductImages").ToList();
            return View(productList);
        }

        public IActionResult Details(int id)
        {
            ShoppingCart shoppingCart = new();
            if (shoppingCart.Product == null)
            {
                shoppingCart = new()
                {
                    Product = _unitOfWork.ProductRepository.Get(u => u.Id == id, includeProperties: "ProductImages,Category"),
                    Count = 1,
                    ProductId = id
                };
            }
            return View(shoppingCart);

        }


        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart shoppingCart) {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            string userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            shoppingCart.ApplicationUserId = userId;
            shoppingCart.Id = 0;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCartRepository.Get(u => u.ApplicationUserId == userId && u.ProductId == shoppingCart.ProductId);

            if(cartFromDb != null)
            {
                cartFromDb.Count += shoppingCart.Count;
                _unitOfWork.ShoppingCartRepository.Update(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                shoppingCart.Id = 0;
                _unitOfWork.ShoppingCartRepository.Add(shoppingCart);
                _unitOfWork.Save();
                HttpContext.Session.SetInt32(SD.SessionCart, _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId).Count());
            }
                

            TempData["success"] = "Product Added to Cart";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}