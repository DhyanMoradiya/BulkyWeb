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
    public class ProductImageRepository : Repository<ProductImage>, IProductImageRepository
    {

        private readonly ApplicationDbContext _db;

        public ProductImageRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(ProductImage productImage)
        {
            _db.productImages.Update(productImage);
        }
    }
}
