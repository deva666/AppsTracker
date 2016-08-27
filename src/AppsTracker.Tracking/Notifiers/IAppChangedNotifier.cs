using System;

namespace AppsTracker.Tracking
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
