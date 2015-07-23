using System;
using System.Threading.Tasks;

namespace AppsTracker.Common.Utils
{
    public interface IWorkQueue : IDisposable
    {
        Task EnqueueWork(Action work);
        Task<Object> EnqueueWork(Func<Object> work);
    }
}
