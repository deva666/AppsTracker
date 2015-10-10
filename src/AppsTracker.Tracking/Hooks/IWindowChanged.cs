using System;

namespace AppsTracker.Tracking.Hooks
{
    public interface IWindowChanged : IDisposable
    {
        event EventHandler<WinChangedArgs> ActiveWindowChanged;
    }
}
