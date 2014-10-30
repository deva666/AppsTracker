using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppsTracker.Service
{
    public interface IBaseService : IDisposable
    {
        void Save();
    }
}
