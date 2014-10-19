using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using AppsTracker.Models.EntityModels;

namespace AppsTracker.DAL.Repos
{
    public sealed class ScreenshotRepo : BaseRepo<Screenshot>, IRepo<Screenshot>
    {
        private static Lazy<ScreenshotRepo> _instance = new Lazy<ScreenshotRepo>(() => new ScreenshotRepo());
        public static ScreenshotRepo Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private ScreenshotRepo() { }

        public IEnumerable<Screenshot> Get()
        {
            using (var context = new AppsEntities())
            {
                return BaseQuery(context).ToList();
            }
        }

        public IEnumerable<Screenshot> Get(params Expression<Func<Screenshot, object>>[] include)
        {
            using (var context = new AppsEntities())
            {
                var query = BaseQuery(context);
                foreach (var incl in include)
                    query = query.Include(incl);

                return query.ToList();
            }
        }

        public IEnumerable<Screenshot> GetFiltered(Expression<Func<Screenshot, bool>> filter)
        {
            using (var context = new AppsEntities())
            {
                return BaseQuery(context).Where(filter).ToList();
            }
        }

        public IEnumerable<Screenshot> GetFiltered(Expression<Func<Screenshot, bool>> filter, params Expression<Func<Screenshot, object>>[] include)
        {
            using (var context = new AppsEntities())
            {
                var query = BaseQuery(context).Where(filter);
                foreach (var incl in include)
                    query = query.Include(incl);

                return query.ToList();
            }
        }

        public Task<IEnumerable<Screenshot>> GetFilteredAsync(Expression<Func<Screenshot, bool>> filter)
        {
            return Task<IEnumerable<Screenshot>>.Run(() => GetFiltered(filter));
        }

        public Task<IEnumerable<Screenshot>> GetFilteredAsync(Expression<Func<Screenshot, bool>> filter, params Expression<Func<Screenshot, object>>[] include)
        {
            return Task<IEnumerable<Screenshot>>.Run(() => GetFiltered(filter, include));
        }

        public Task<IEnumerable<Screenshot>> GetAsync(params Expression<Func<Screenshot, object>>[] include)
        {
            return Task<IEnumerable<Screenshot>>.Run(() => Get(include));
        }

        protected override IQueryable<Screenshot> BaseQuery(AppsEntities context)
        {
            return (from u in context.Users.AsNoTracking()
                    join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                    join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                    join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                    join s in context.Screenshots.AsNoTracking() on l.LogID equals s.LogID
                    select s);
        }
    }
}
