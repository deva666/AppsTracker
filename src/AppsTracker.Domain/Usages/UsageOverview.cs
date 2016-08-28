using System;
using System.Collections.ObjectModel;

namespace AppsTracker.Domain.Usages
{
    public sealed class UsageOverview
    {
        public string Date { get; set; }
        public DateTime DateInstance { get; set; }
        public ObservableCollection<UsageSummary> UsageCollection { get; set; }
    }
}
