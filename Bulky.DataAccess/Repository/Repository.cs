using Bulky.DataAccess.Repository.IRepositoy;
using BulkyWeb.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {

        private readonly ApplicationDbContext _db;
        internal DbSet<T> dbSet ;

        public Repository(ApplicationDbContext db)
        {
            _db = db;
            dbSet = _db.Set<T>();
        }
        public void Add(T item)
        {
            dbSet.Add(item);
        }

        public T Get(Expression<Func<T, bool>> filter, string? includeProperties = null, bool track = false)
        {
            IQueryable<T> query;
            if (track)
            {
                query = dbSet;
            }
            else
            {
                query = dbSet.AsNoTracking();
            }
            
            if (!(string.IsNullOrEmpty(includeProperties)))
            {
                foreach (var property in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(property);
                }
            }
            query = query.Where(filter);
            return query.FirstOrDefault();
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter, string? includeProperties = null)
        {
            IQueryable<T> query = dbSet;
            if(filter != null)
            {
                query = query.Where(filter);
            }
            if (!(string.IsNullOrEmpty(includeProperties)))
            {
                foreach (var property in includeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
                {
                   query =  query.Include(property);
                }
            }
 
            return query.ToList();
        }

        public void Remove(T item)
        {
            dbSet.Remove(item);
        }

        public void RemoveRange(List<T> item)
        {
            dbSet.RemoveRange(item);
        }
    }
}
