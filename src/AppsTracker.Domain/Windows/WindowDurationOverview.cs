using System.Collections.Generic;

namespace AppsTracker.Domain.Windows
{
    public sealed class WindowDurationOverview
    {
        public string Date { get; set; }
        public List<WindowDuration> DurationCollection { get; set; }
    }
}
