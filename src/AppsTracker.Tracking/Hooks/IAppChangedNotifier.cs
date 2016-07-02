using System;

namespace AppsTracker.Tracking.Hooks
{
    public interface IAppChangedNotifier : IDisposable
    {
        IObservable<AppChangedArgs> AppChangedObservable
        {
            get;
        }

        void CheckActiveApp();
    }
}
