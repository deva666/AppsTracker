using System;

namespace AppsTracker.Tracking.Hooks
{
    public interface ITitleChangedNotifier : IDisposable
    {
        event EventHandler<WinChangedArgs> TitleChanged;
    }
}
