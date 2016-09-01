using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Tracking.Usage
{
    public enum UsageAction
    {
        Start,
        Stop
    }

    public struct UsageEvent
    {
        public UsageAction Action { get; }

        public UsageTypes Type { get; }
    }
}
