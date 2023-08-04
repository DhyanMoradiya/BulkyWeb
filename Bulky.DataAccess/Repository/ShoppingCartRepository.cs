using Bulky.DataAccess.Repository.IRepositoy;
using Bulky.Model.Models;
using BulkyWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
        private readonly ApplicationDbContext _db;
        public ShoppingCartRepository(ApplicationDbContext db) : base(db)
        {
                _db = db;   
        }

        public void Update(ShoppingCart shoppingCart)
        {
            _db.shoppingCarts.Update(shoppingCart);   
        }
    }
}
