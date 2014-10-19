using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Models.EntityModels;

namespace AppsTracker.DAL.Repos
{
    public sealed class AplicationRepo : BaseRepo<Aplication>, IRepo<Aplication>
    {
        private static Lazy<AplicationRepo> _instance = new Lazy<AplicationRepo>(() => new AplicationRepo());
        public static AplicationRepo Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        private AplicationRepo() { }

        public IEnumerable<Aplication> Get()
        {
            using (var context = new AppsEntities())
            {
                return BaseQuery(context).ToList().Distinct(new AppComparer());
            }
        }

        public IEnumerable<Aplication> GetFiltered(Expression<Func<Aplication, bool>> filter)
        {
            using (var context = new AppsEntities())
            {
                return BaseQuery(context).Where(filter).ToList().Distinct(new AppComparer());
            }
        }

        public Task<IEnumerable<Aplication>> GetFilteredAsync(Expression<Func<Aplication, bool>> filter)
        {
            return Task<IEnumerable<Aplication>>.Run(() => GetFiltered(filter));
        }

        private class AppComparer : IEqualityComparer<Aplication>
        {

            public bool Equals(Aplication x, Aplication y)
            {
                return x.ApplicationID == y.ApplicationID;
            }

            public int GetHashCode(Aplication obj)
            {
                return obj.GetHashCode();
            }
        }

        protected override IQueryable<Aplication> BaseQuery(AppsEntities context)
        {
            return (from u in context.Users.AsNoTracking()
                    join a in context.Applications.AsNoTracking() on u.UserID equals a.UserID
                    join w in context.Windows.AsNoTracking() on a.ApplicationID equals w.ApplicationID
                    join l in context.Logs.AsNoTracking() on w.WindowID equals l.WindowID
                    select a);
        }
    }
}
