using System;

namespace AppsTracker.Tracking.Usage
{
    interface IUsageNotifier
    {
        IObservable<UsageEvent> UsageObservable { get; }

        void Init();
    }
}
