using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using AppsTracker.Models.EntityModels;

namespace AppsTracker.DAL.Repos
{
    public abstract class BaseRepo<TEntity>
    {
        protected abstract IQueryable<TEntity> BaseQuery(AppsEntities context);      
    }
}
