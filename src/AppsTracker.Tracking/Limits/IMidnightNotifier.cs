using System;

namespace AppsTracker.Tracking.Limits
{
    public interface IMidnightNotifier : IDisposable
    {
        event EventHandler MidnightTick;
    }
}
