using System;

namespace AppsTracker.Tracking
{
    public interface ITitleChangedNotifier : IDisposable
    {
        IObservable<WinChangedArgs> TitleChangedObservable
        {
            get;
        }
    }
}
