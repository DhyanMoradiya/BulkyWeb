using Bulky.DataAccess.Repository.IRepositoy;
using Bulky.Model.Models;
using Bulky.Model.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BulkyWeb.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public ShoppingCartVM shoppingCartVM { get; set; }

		public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            string userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties:"Product"),
                OrderHeader = new()
            };

            foreach(ShoppingCart cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            return View(shoppingCartVM);
        }

        public IActionResult Plus(int cartId) {
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCartRepository.Get(u => u.Id == cartId);
            cartFromDb.Count += 1;
            _unitOfWork.ShoppingCartRepository.Update(cartFromDb);
            _unitOfWork.Save();
            
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Minus(int cartId)
        {
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCartRepository.Get(u => u.Id == cartId);
            if(cartFromDb.Count <= 1)
            {
                _unitOfWork.ShoppingCartRepository.Remove(cartFromDb);
            }
            else
            {
                cartFromDb.Count -= 1;
                _unitOfWork.ShoppingCartRepository.Update(cartFromDb);
            }
            
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Remove(int cartId)
        {
            ShoppingCart cartFromDb = _unitOfWork.ShoppingCartRepository.Get(u => u.Id == cartId);
            _unitOfWork.ShoppingCartRepository.Remove(cartFromDb);
            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary() {
            var claimIdentity = (ClaimsIdentity)User.Identity;
            string userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM = new()
            {
                ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };

            foreach (ShoppingCart cart in shoppingCartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
            }

            shoppingCartVM.OrderHeader.ApplicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);

            shoppingCartVM.OrderHeader.Name = shoppingCartVM.OrderHeader.ApplicationUser.Name;
            shoppingCartVM.OrderHeader.City = shoppingCartVM.OrderHeader.ApplicationUser.City;
            shoppingCartVM.OrderHeader.PostalCode = shoppingCartVM.OrderHeader.ApplicationUser.PostalCode;
            shoppingCartVM.OrderHeader.PhoneNumber = shoppingCartVM.OrderHeader.ApplicationUser.PhoneNumber;
            shoppingCartVM.OrderHeader.State = shoppingCartVM.OrderHeader.ApplicationUser.State;
            shoppingCartVM.OrderHeader.StreetAddress = shoppingCartVM.OrderHeader.ApplicationUser.StreetAddress;

            return View(shoppingCartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
		public IActionResult SummaryPOST()
		{
			var claimIdentity = (ClaimsIdentity)User.Identity;
			string userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            shoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");


			foreach (ShoppingCart cart in shoppingCartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				shoppingCartVM.OrderHeader.OrderTotal += (cart.Price * cart.Count);
			}

			ApplicationUser applicationUser = _unitOfWork.ApplicationUserRepository.Get(u => u.Id == userId);
            shoppingCartVM.OrderHeader.ApplicationUserId = userId;


			if (applicationUser.CompanyId == 0)
            {
                //Reguler customer account need to catture payment
                shoppingCartVM.OrderHeader.OrderStatus = SD.StatusPending;
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusPending;
            }
            else
            {
				//Company user
				shoppingCartVM.OrderHeader.OrderStatus = SD.StatusApproved;
                shoppingCartVM.OrderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
			}

            _unitOfWork.OrderHeaderRepository.Add(shoppingCartVM.OrderHeader);
            _unitOfWork.Save();

            foreach(ShoppingCart cart in shoppingCartVM.ShoppingCartList)
            {
                OrderDetail orderDetail = new() { 
				    ProductId = cart.ProductId,
                    OrderHeaderId = shoppingCartVM.OrderHeader.Id,
                    Price = cart.Price,
					Count = cart.Count
				};

                _unitOfWork.OrderDetailRepository.Add(orderDetail);
                _unitOfWork.Save();
			}

            if(applicationUser.CompanyId == 0)
            {
                //regulor customer user
                //add strip logic
            }
            else
            {
                //company user
                return RedirectToAction(nameof(OrderConformation), new {id = shoppingCartVM.OrderHeader.Id});
            }

			return View(shoppingCartVM);
		}

        public IActionResult OrderConformation(int id)
        {
            return View(id);
        }


		private double GetPriceBasedOnQuantity(ShoppingCart shoppingCart)
        {
            if(shoppingCart.Count < 50)
            {
                return shoppingCart.Product.Price;
            }else if(shoppingCart.Count < 100)
            {
                return shoppingCart.Product.Price50;
            }
            else
            {
                return shoppingCart.Product.Price100;
            }
        }
    }
}
