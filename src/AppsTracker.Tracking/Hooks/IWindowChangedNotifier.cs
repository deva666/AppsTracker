using System;

namespace AppsTracker.Tracking.Hooks
{
    public interface IWindowChangedNotifier : IDisposable
    {
        event EventHandler<WinChangedArgs> ActiveWindowChanged;
    }
}
