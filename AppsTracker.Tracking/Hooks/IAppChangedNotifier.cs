using System;

namespace AppsTracker.Tracking.Hooks
{
    public interface IAppChangedNotifier : IDisposable
    {
        event EventHandler<AppChangedArgs> AppChanged;
    }
}
