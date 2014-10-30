using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Linq.Expressions;

namespace AppsTracker.DAL.Repos
{
    //public abstract class GenericRepository<TEntity> : IRepository<TEntity> where TEntity : class
    //{
    //    private DbContext _context;

    //    public GenericRepository(DbContext context)
    //    {
    //        _context = context;
    //    }

    //    public IQueryable<TEntity> GetQueryable()
    //    {
    //        return _context.Set<TEntity>();
    //    }

    //    public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter)
    //    {
    //        return _context.Set<TEntity>().Where(filter).ToList();
    //    }
      
    //    public IEnumerable<TEntity> GetAll()
    //    {
    //        return _context.Set<TEntity>().ToList();
    //    }

    //    public TEntity GetSingle(Expression<Func<TEntity, bool>> filter)
    //    {
    //        return _context.Set<TEntity>().Where(filter).SingleOrDefault();
    //    }
       
    //    public TEntity GetByID(int id)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}
