using System;

namespace AppsTracker.Tracking.Hooks
{
    public interface IWindowChangedNotifier : IDisposable
    {
        IObservable<WinChangedArgs> WinChangedObservable
        {
            get;
        }
    }
}
