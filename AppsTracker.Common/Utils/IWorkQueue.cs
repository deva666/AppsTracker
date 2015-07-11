using System;

namespace AppsTracker.Common.Utils
{
    public interface IWorkQueue : IDisposable
    {
        void EnqueueWork(Action work);
    }
}
