using System;

namespace AppsTracker.Tracking
{
    public interface IWindowChangedNotifier : IDisposable
    {
        IObservable<WinChangedArgs> WinChangedObservable
        {
            get;
        }
    }
}
