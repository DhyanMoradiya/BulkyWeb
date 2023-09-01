using Bulky.Model.Models;
using BulkyWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepositoy
{
    public interface IProductImageRepository : IRepository<ProductImage>
    {
        void Update(ProductImage productImage);
    }
}
