using System;
namespace AppsTracker.Tracking.Helpers
{
    interface IMidnightNotifier
    {
        event EventHandler MidnightTick;
    }
}
