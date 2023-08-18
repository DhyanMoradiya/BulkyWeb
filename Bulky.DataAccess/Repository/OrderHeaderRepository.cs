using Bulky.DataAccess.Repository.IRepositoy;
using Bulky.Model.Models;
using BulkyWeb.Data;
using BulkyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
	public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {

        private readonly ApplicationDbContext _db;

        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(OrderHeader orderHeader)
        {
            _db.orderHeaders.Update(orderHeader);
        }

		void IOrderHeaderRepository.UpdateStatus(int id, string orderStatus, string? paymentStatus)
		{
			var orderHeaderFromDb = _db.orderHeaders.FirstOrDefault(x => x.Id == id);
			if(orderHeaderFromDb != null)
			{
				orderHeaderFromDb.OrderStatus = orderStatus;
				if(paymentStatus != null)
				{
					orderHeaderFromDb.PaymentStatus = paymentStatus;
				}
			}
		}

		void IOrderHeaderRepository.UpdateStripePaymentId(int id, string sessionId, string paymentIntentId)
		{
			var orderHeaderFromDb = _db.orderHeaders.FirstOrDefault(x => x.Id == id);
			if (sessionId != null)
			{
				orderHeaderFromDb.SessionId = sessionId;
			}
			if(!string.IsNullOrEmpty(paymentIntentId))
			{
				orderHeaderFromDb.PaymentIntentId = paymentIntentId;
				orderHeaderFromDb.PaymentDate = DateTime.Now;
			}
		}
	}
}
