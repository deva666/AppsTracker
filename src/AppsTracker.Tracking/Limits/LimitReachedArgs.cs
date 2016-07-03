using System;
using AppsTracker.Data.Models;

namespace AppsTracker.Tracking.Limits
{
    internal sealed class LimitReachedArgs : EventArgs
    {
        public AppLimit Limit
        {
            get;
            private set;
        }

        public LimitReachedArgs(AppLimit limit)
        {
            Limit = limit;
        }
    }
}
