using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Models
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        myContext ef;
        DbSet<TEntity> dbSet;

        public GenericRepository(myContext _ef)
        {
            ef = _ef;
            dbSet = ef.Set<TEntity>();
        }

        public bool Insert(TEntity record)
        {
            try
            {
                dbSet.Add(record);
                ef.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool Delete(TEntity record)
        {
            try
            {
                dbSet.Remove(record);
                ef.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool Delete(int id)
        {
            try
            {
                dbSet.Remove(Select(id));
                ef.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public bool Update(TEntity record)
        {
            try
            {
                dbSet.Attach(record);
                ef.Entry(record).State = System.Data.Entity.EntityState.Modified;
                ef.SaveChanges();
                return true;
            }
            catch { return false; }
        }

        public TEntity Select(int id)
        {
            try
            {
                return dbSet.Find(id);
            }
            catch { return null; }
        }

        public IEnumerable<TEntity> Select(Expression<Func<TEntity, bool>> where = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderby = null)
        {
            try
            {
                IQueryable<TEntity> query = dbSet;

                if (where != null)
                {
                    query = query.Where(where);
                }

                if (orderby != null)
                {
                    query = orderby(query);
                }

                return query.ToList();
            }
            catch(Exception ttt) { string h = ttt.Message; return null; }
        }

        public int Count(Expression<Func<TEntity, bool>> where = null)
        {
            try
            {
                if (where != null)
                {
                    return dbSet.Where(where).Count();
                }
                return dbSet.Count();
            }
            catch { return 0; }
        }

    }
}
