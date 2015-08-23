using System;

namespace AppsTracker.Tracking.Hooks
{
    public interface IWinChanged : IDisposable
    {
        event EventHandler<WinChangedArgs> ActiveWindowChanged;
    }
}
