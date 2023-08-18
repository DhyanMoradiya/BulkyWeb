using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepositoy
{
    public interface IUnitOfWork
    {
        ICategoryRepository CategoryRepository { get; }
        IProductRepository ProductRepository { get; }

        ICompanyRepository CompanyRepository { get; }

        IShoppingCartRepository ShoppingCartRepository { get; } 
        IApplicationUserRepository ApplicationUserRepository { get; }

        IOrderHeaderRepository OrderHeaderRepository { get; }
        IOrderDetailRepository OrderDetailRepository { get; }
        void Save();
    }
}
