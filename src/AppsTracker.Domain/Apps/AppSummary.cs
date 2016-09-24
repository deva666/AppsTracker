using System;
using AppsTracker.Domain.Model;

namespace AppsTracker.Domain.Apps
{
    public sealed class AppSummary : SummaryBase
    {
        public string AppName { get; set; }
        public string Date { get; set; }
        public DateTime DateTime { get; set; }
    }
}
