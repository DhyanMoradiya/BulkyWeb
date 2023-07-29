using Bulky.DataAccess.Repository.IRepositoy;
using Bulky.Model.Models;
using BulkyWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

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
            List<Product> productList = _unitOfWork.ProductRepository.GetAll(includeProperties : "Category").ToList();
            return View(productList);
        }

        public IActionResult Details(int? id)
        {
            if(id == null) { 
            return NotFound();
            }
            Product product = _unitOfWork.ProductRepository.Get(u => u.Id == id , includeProperties: "Category");
            return View(product);
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