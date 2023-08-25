using Bulky.DataAccess.Repository;
using Bulky.DataAccess.Repository.IRepositoy;
using Bulky.Model.Models;
using Bulky.Model.ViewModels;
using Bulky.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Diagnostics;
using System.Security.Claims;

namespace BulkyWeb.Areas.Admin.Controllers
{
	[Area(nameof(Admin))]
    [Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }

		public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
		{
			return View();
		}

        public IActionResult Details(int orderId)
        {
            OrderVM = new OrderVM
            {
                orderHeader = _unitOfWork.OrderHeaderRepository.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                orderDetails = _unitOfWork.OrderDetailRepository.GetAll(u => u.OrderHeaderId == orderId, includeProperties:"Product")
            };

            return View(OrderVM);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + " , "+ SD.Role_Employee)]
        public IActionResult UpdateOrderDetail()
        {
            OrderHeader orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.Get(u => u.Id == OrderVM.orderHeader.Id);
            orderHeaderFromDb.Name = OrderVM.orderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.orderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.orderHeader.StreetAddress;   
            orderHeaderFromDb.City = OrderVM.orderHeader.City;
            orderHeaderFromDb.State = OrderVM.orderHeader.State;
            orderHeaderFromDb.PostalCode = OrderVM.orderHeader.PostalCode;
            if (!string.IsNullOrEmpty(OrderVM.orderHeader.TrakingNumber))
            {
                orderHeaderFromDb.TrakingNumber = OrderVM.orderHeader.TrakingNumber;
            }
            if (!string.IsNullOrEmpty(OrderVM.orderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier = OrderVM.orderHeader.Carrier;
            }
            _unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order detail updated successfully";

            return RedirectToAction(nameof(Details), new {orderId = OrderVM.orderHeader.Id});
        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + " , " + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.orderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();

            TempData["Success"] = "Order status updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }


        public IActionResult ShipOrder()
        {
            OrderHeader orderHeaderFromDb = _unitOfWork.OrderHeaderRepository.Get(u => u.Id == OrderVM.orderHeader.Id);
            orderHeaderFromDb.TrakingNumber = OrderVM.orderHeader.TrakingNumber;
            orderHeaderFromDb.Carrier = OrderVM.orderHeader.Carrier;
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            orderHeaderFromDb.ShippingDate = DateTime.Now;
            if(orderHeaderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            _unitOfWork.OrderHeaderRepository.Update(orderHeaderFromDb);
            _unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.orderHeader.Id, SD.StatusShipped);
            _unitOfWork.Save();

            TempData["Success"] = "Order status updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }

        public IActionResult CancelOrder()
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.Get(u => u.Id == OrderVM.orderHeader.Id);
            if (orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.orderHeader.Id, SD.StatusCancelled);
            }
            else
            {
                _unitOfWork.OrderHeaderRepository.UpdateStatus(OrderVM.orderHeader.Id, SD.StatusCancelled);
            }
            _unitOfWork.Save();

            TempData["Success"] = "Order canceled updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }


        [ActionName(nameof(Details))]
        [HttpPost]
        public IActionResult Details_Pay_now()
        {
            OrderVM.orderHeader = _unitOfWork.OrderHeaderRepository.Get(u=> u.Id == OrderVM.orderHeader.Id, includeProperties:"ApplicationUser");
            OrderVM.orderDetails = _unitOfWork.OrderDetailRepository.GetAll(u => u.OrderHeaderId == OrderVM.orderHeader.Id, includeProperties : "Product");

            if(OrderVM.orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                //add strip logic
                var domain = "https://localhost:7067/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"Admin/Order/OrderConformation?orderId={OrderVM.orderHeader.Id}",
                    CancelUrl = domain + $"Admin/Order/Details?orderId={OrderVM.orderHeader.Id}",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

                foreach (OrderDetail detail in OrderVM.orderDetails)
                {
                    SessionLineItemOptions sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(detail.Price * 100),
                            Currency = "INR",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = detail.Product.Title
                            },
                        },
                        Quantity = detail.Count
                    };

                    options.LineItems.Add(sessionLineItem);
                }

                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(OrderVM.orderHeader.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            }

            return View();
        }

        public IActionResult OrderConformation(int orderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeaderRepository.Get(u=>u.Id == orderId);

            var service = new SessionService();
            Session session = service.Get(orderHeader.SessionId);

            if (session.PaymentStatus.ToLower() == "paid")
            {
                _unitOfWork.OrderHeaderRepository.UpdateStatus(orderHeader.Id, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                _unitOfWork.OrderHeaderRepository.UpdateStripePaymentId(orderHeader.Id, orderHeader.SessionId, session.PaymentIntentId);
                _unitOfWork.Save();
            }

        

            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCartRepository.GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.ShoppingCartRepository.RemoveRange(shoppingCarts);
            _unitOfWork.Save();

            return View(orderId);

        }

        #region API CALLS

        [HttpGet]
	public IActionResult GetAll(string status)
	{
            IEnumerable<OrderHeader> orderHeaderList;


            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee)){
                orderHeaderList = _unitOfWork.OrderHeaderRepository.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimIdentity = (ClaimsIdentity)User.Identity;
                string userId = claimIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                orderHeaderList = _unitOfWork.OrderHeaderRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }



            switch (status)
            {
                case "pending":
                    orderHeaderList = orderHeaderList.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                    break;

                case "inprocess":
                    orderHeaderList = orderHeaderList.Where(u => u.OrderStatus == SD.StatusInProcess);
                    break;

                case "completed":
                    orderHeaderList = orderHeaderList.Where(u => u.OrderStatus == SD.StatusShipped);
                    break;

                case "approved":
                    orderHeaderList = orderHeaderList.Where(u => u.OrderStatus == SD.StatusApproved);
                    break;  

                default:
                    break;
            }


            return Json(new { data = orderHeaderList });
    }

    }
    #endregion
}
