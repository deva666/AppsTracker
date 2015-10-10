using System;

namespace AppsTracker.Tracking.Helpers
{
    public interface IMidnightNotifier : IDisposable
    {
        event EventHandler MidnightTick;
    }
}
