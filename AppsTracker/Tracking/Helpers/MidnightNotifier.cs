using System;
using System.ComponentModel.Composition;
using System.Threading;

namespace AppsTracker.Tracking.Helpers
{
    [Export(typeof(IMidnightNotifier))]
    internal sealed class MidnightNotifier : AppsTracker.Tracking.Helpers.IMidnightNotifier
    {
        public event EventHandler MidnightTick;

        private Timer timer;

        public MidnightNotifier()
        {
            InitializeTimer();
        }

        private void InitializeTimer()
        {
            var midnight = DateTime.Now.Date.AddDays(1);
            var ticksToMidnight = midnight.Ticks - DateTime.Now.Ticks;
            timer = new Timer(TimerTick, null, new TimeSpan(ticksToMidnight), Timeout.InfiniteTimeSpan);
        }

        private void TimerTick(object state)
        {
            MidnightTick.InvokeSafely(this, EventArgs.Empty);
            InitializeTimer();
        }
    }
}
