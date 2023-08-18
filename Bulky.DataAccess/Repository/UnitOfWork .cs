using Bulky.DataAccess.Repository.IRepositoy;
using BulkyWeb.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        public ICategoryRepository CategoryRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }
        public ICompanyRepository CompanyRepository { get; private set; }
         public IShoppingCartRepository ShoppingCartRepository { get; private set; }
        public IApplicationUserRepository ApplicationUserRepository { get; private set; }
        public IOrderDetailRepository OrderDetailRepository { get; private set; }
        public IOrderHeaderRepository OrderHeaderRepository { get; private set; }

        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db)
        {
            _db = db;

            CategoryRepository = new CategoryRepository(db);
            ProductRepository = new ProductRepository(db);
            CompanyRepository = new CompanyRepository(db);
            ShoppingCartRepository = new ShoppingCartRepository(db);
            ApplicationUserRepository = new ApplicationUserRepository(db);
            OrderDetailRepository = new OrderDetailRepository(db);  
            OrderHeaderRepository = new OrderHeaderRepository(db);
        }

        public void Save()
        {
                _db.SaveChanges();
        }
    }
}
