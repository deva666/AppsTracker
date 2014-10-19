using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using AppsTracker.Models.EntityModels;
using System.Linq.Expressions;

namespace AppsTracker.DAL.Repos
{
    public sealed class UsageRepo : BaseRepo<Usage>, IRepo<Usage>
    {
        private static Lazy<UsageRepo> _instance = new Lazy<UsageRepo>(() => new UsageRepo());
        public static UsageRepo Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private UsageRepo() { }

        public IEnumerable<Usage> Get()
        {
            using (var context = new AppsEntities())
            {
                return BaseQuery(context).ToList();
            }
        }

        public IEnumerable<Usage> Get(params Expression<Func<Usage,object>>[] include)
        {
            using (var context = new AppsEntities())
            {
                var query = BaseQuery(context);
                foreach (var incl in include)
                    query = query.Include(incl);

                return query.ToList();
            }
        }

        public IEnumerable<Usage> GetFiltered(Expression<Func<Usage, bool>> filter)
        {
            using (var context =  new AppsEntities())
            {
                return BaseQuery(context).Where(filter).ToList();
            }
        }

        public IEnumerable<Usage> GetFiltered(Expression<Func<Usage, bool>> filter, params Expression<Func<Usage, object>>[] include)
        {
            using (var context = new AppsEntities())
            {
                var query = BaseQuery(context).Where(filter);
                foreach (var incl in include)
                    query = query.Include(incl);

                return query.ToList();
            }
        }

        public Task<IEnumerable<Usage>> GetAsync()
        {
            return Task<IEnumerable<Usage>>.Run(() => Get());
        }

        public Task<IEnumerable<Usage>> GetAsync(params Expression<Func<Usage, object>>[] include)
        {
            return Task<IEnumerable<Usage>>.Run(() => Get(include));
        }

        public Task<IEnumerable<Usage>> GetFilteredAsync(Expression<Func<Usage, bool>> filter)
        {
            return Task<IEnumerable<Usage>>.Run(() => GetFiltered(filter));
        }

        public Task<IEnumerable<Usage>> GetFilteredAsync(Expression<Func<Usage, bool>> filter, params Expression<Func<Usage, object>>[] include)
        {
            return Task<IEnumerable<Usage>>.Run(() => GetFiltered(filter, include));
        }

        protected override IQueryable<Usage> BaseQuery(AppsEntities context)
        {
            return (from u in context.Users.AsNoTracking()
                    join us in context.Usages.AsNoTracking() on u.UserID equals us.UserID
                    select us);
        }
    }
}
