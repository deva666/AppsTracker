using System;

namespace AppsTracker.Tracking.Hooks
{
    public interface ITitleChanged : IDisposable
    {
        event EventHandler<WinChangedArgs> TitleChanged;
    }
}
