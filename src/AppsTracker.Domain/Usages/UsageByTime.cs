using System.Collections.ObjectModel;

namespace AppsTracker.Domain.Usages
{
    public sealed class UsageByTime
    {
        public string Time { get; set; }
        public ObservableCollection<UsageSummary> UsageSummaryCollection { get; set; }
    }
}
