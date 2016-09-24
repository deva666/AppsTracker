using System.Collections.Generic;

namespace AppsTracker.Domain.Apps
{
    public sealed class AppDurationOverview
    {
        public string Date { get; set; }
        public List<AppDuration> AppCollection { get; set; }
    }
}
