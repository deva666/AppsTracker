using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.DAL.Repos
{
    public interface IRepositoryFactory
    {
        T Get<T>() where T : class;
    }
}
