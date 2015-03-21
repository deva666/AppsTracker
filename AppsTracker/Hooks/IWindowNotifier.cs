using System;

namespace AppsTracker.Hooks
{
    public interface IWindowNotifier : IDisposable
    {
        event EventHandler<WindowChangedArgs> WindowChanged;
    }
}
