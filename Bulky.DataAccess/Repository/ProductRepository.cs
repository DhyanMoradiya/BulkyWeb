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
    public class ProductRepository : Repository<Product>, IProductRepository
    {

        private readonly ApplicationDbContext _db;
        public ProductRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void Update(Product product)
        {
            Product productFormDb = _db.Products.FirstOrDefault(x => x.Id == product.Id);
            if (productFormDb != null)
            {
                productFormDb.Title = product.Title;
                productFormDb.Description = product.Description;   
                productFormDb.Price = product.Price;    
                productFormDb.ListPrice = product.ListPrice;
                productFormDb.Price100 = product.Price100;
                productFormDb.Price50 = product.Price50;
                productFormDb.ISBN = product.ISBN;
                productFormDb.Author = product.Author;
                productFormDb.CategoryId = product.CategoryId;
                if(product.ImageURL != null)
                {
                    productFormDb.ImageURL = product.ImageURL;
                }
            }
           _db.Products.Update(productFormDb);
        }
    }
}
