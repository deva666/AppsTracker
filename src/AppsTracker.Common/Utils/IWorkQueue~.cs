using System;
using System.Threading.Tasks;

namespace AppsTracker.Common.Utils
{
    public interface IWorkQueue<T> : IDisposable
    {
        Task<T> EnqueueWork(Func<T> work);
    }
}
