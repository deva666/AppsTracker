using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.DAL.Repos
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IQueryable<TEntity> GetQueryable();
        IEnumerable<TEntity> GetAll();
        IEnumerable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] navigation);
        IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter);
        IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] navigation);
        TEntity GetSingle(Expression<Func<TEntity, bool>> filter);
        TEntity GetByID(int id);
    }
}
