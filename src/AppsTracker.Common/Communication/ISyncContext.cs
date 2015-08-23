using System;
using System.Threading;

namespace AppsTracker.Communication
{
    public interface ISyncContext
    {
        SynchronizationContext Context { get; set; }

        void Invoke(SendOrPostCallback method, Object state = null);

        void Invoke(Action action);
    }
}
