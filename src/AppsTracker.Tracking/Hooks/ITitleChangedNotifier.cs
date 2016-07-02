using System;

namespace AppsTracker.Tracking.Hooks
{
    public interface ITitleChangedNotifier : IDisposable
    {
        IObservable<WinChangedArgs> TitleChangedObservable
        {
            get;
        }
    }
}
