using System;

namespace AppsTracker.Tracking
{
    interface IIdleNotifier : IDisposable
    {
        event EventHandler IdleEntered;
        event EventHandler IdleStoped;
    }
}
