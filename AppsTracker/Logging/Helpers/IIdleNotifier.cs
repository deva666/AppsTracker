using System;

namespace AppsTracker.Logging
{
    interface IIdleNotifier : IDisposable
    {
        event EventHandler IdleEntered;
        event EventHandler IdleStoped;
    }
}
