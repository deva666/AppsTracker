using System;

namespace AppsTracker.Tracking.Helpers
{
    public interface IMidnightNotifier
    {
        event EventHandler MidnightTick;
    }
}
