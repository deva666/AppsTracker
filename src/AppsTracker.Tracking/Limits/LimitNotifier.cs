using System;
using System.Threading;
using AppsTracker.Communication;
using AppsTracker.Data.Models;

namespace AppsTracker.Tracking.Limits
{
    internal sealed class LimitNotifier : IDisposable
    {
        public event EventHandler<LimitReachedArgs> LimitReached = delegate { };

        private readonly Timer timer;
        private readonly ISyncContext syncContext;

        public LimitSpan LimitSpan
        {
            get;
            private set;
        }

        public AppLimit Limit
        {
            get;
            private set;
        }

        public LimitNotifier(ISyncContext syncContext, LimitSpan limitSpan)
        {
            this.syncContext = syncContext;
            this.LimitSpan = limitSpan;
            this.timer = new Timer(OnTimerTick, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        private void OnTimerTick(object state)
        {
            syncContext.Invoke(() =>
            {
                LimitReached(this, new LimitReachedArgs(Limit));
            });
        }

        public void Setup(AppLimit limit, TimeSpan duration)
        {
            Limit = limit;
            timer.Change(duration, Timeout.InfiniteTimeSpan);
        }

        public void Stop()
        {
            timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        }

        public void Dispose()
        {
            timer.Dispose();
        }
    }
}
