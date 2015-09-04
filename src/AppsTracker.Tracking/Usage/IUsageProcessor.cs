using System;
namespace AppsTracker.Tracking.Helpers
{
    internal interface IUsageProcessor
    {
        void NewUsage(AppsTracker.Data.Models.UsageTypes usageType);

        void UsageEnded(AppsTracker.Data.Models.UsageTypes usageType);

        void EndAllUsages();
    }
}
