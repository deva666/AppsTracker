using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
