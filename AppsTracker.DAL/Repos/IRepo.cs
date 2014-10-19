using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.DAL.Repos
{
    interface IRepo<T>
    {
        IEnumerable<T> Get();
        IEnumerable<T> GetFiltered(Expression<Func<T, bool>> filter);
    }
}
