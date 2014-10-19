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
    public sealed class LogRepo : BaseRepo<Log>, IRepo<Log>
    {
        private static Lazy<LogRepo> _instance = new Lazy<LogRepo>(() => new LogRepo());
        public static LogRepo Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private LogRepo() { }

        public IEnumerable<Log> Get()
        {
            using (var context = new AppsEntities())
            {
                return BaseQuery(context).ToList();
            }
        }

        public IEnumerable<Log> Get(params Expression<Func<Log, object>>[] include)
        {
            using (var context = new AppsEntities())
            {
                IQueryable<Log> query = BaseQuery(context);
                foreach (var incl in include)
                    query = query.Include(incl);
                return query.ToList();
            }
        }

        public IEnumerable<Log> GetFiltered(Expression<Func<Log, bool>> filter)
        {
            using (var context = new AppsEntities())
            {
                return BaseQuery(context).Where(filter).ToList();
            }
        }

        public IEnumerable<Log> GetFiltered(Expression<Func<Log, bool>> filter, params Expression<Func<Log, object>>[] include)
        {
            using (var context = new AppsEntities())
            {
                IQueryable<Log> query = BaseQuery(context).Where(filter);
                foreach (var incl in include)
                    query = query.Include(incl);

                return query.ToList();
            }
        }

        public Task<IEnumerable<Log>> GetAsync()
        {
            return Task<IEnumerable<Log>>.Run(new Func<IEnumerable<Log>>(Get));
        }

        public Task<IEnumerable<Log>> GetAsync(params Expression<Func<Log, object>>[] include)
        {
            return Task<IEnumerable<Log>>.Run(() => Get(include));
        }

        public Task<IEnumerable<Log>> GetFilteredAsync(Expression<Func<Log, bool>> filter)
        {
            return Task<IEnumerable<Log>>.Run(() => GetFiltered(filter));
        }

        public Task<IEnumerable<Log>> GetFilteredAsync(Expression<Func<Log, bool>> filter, params Expression<Func<Log, object>>[] include)
        {
            return Task<IEnumerable<Log>>.Run(() => GetFiltered(filter, include));
        }



        protected override IQueryable<Log> BaseQuery(AppsEntities context)
        {
            return (from u in context.Users.AsNoTracking()
                    join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                    join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                    join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                    select l);
        }
    }
}
