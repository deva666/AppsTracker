using System;

namespace AppsTracker.Hooks
{
    public interface IWindowChangedNotifier : IDisposable
    {
        event EventHandler<WindowChangedArgs> WindowChanged;
    }
}
