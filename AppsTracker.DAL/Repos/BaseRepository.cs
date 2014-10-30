using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.DAL.Repos
{
    public abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        
        protected BaseRepository()
        {
            
        }

        public virtual IQueryable<TEntity> GetQueryable()
        {
            using (var context = new AppsEntities())
            {
                return context.Set<TEntity>();
            }
        }

        public virtual IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<TEntity> GetAll()
        {
            throw new NotImplementedException();
        }

        public virtual TEntity GetSingle(Expression<Func<TEntity, bool>> filter)
        {
            throw new NotImplementedException();
        }

        public abstract TEntity GetByID(int id);


        public IEnumerable<TEntity> GetAll(params Expression<Func<TEntity, object>>[] navigation)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TEntity> GetFiltered(Expression<Func<TEntity, bool>> filter, params Expression<Func<TEntity, object>>[] navigation)
        {
            throw new NotImplementedException();
        }
    }
}
