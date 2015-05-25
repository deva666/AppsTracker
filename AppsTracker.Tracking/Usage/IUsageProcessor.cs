using System;
namespace AppsTracker.Tracking.Helpers
{
    internal interface IUsageProcessor
    {
        void RegisterUsage(AppsTracker.Data.Models.Usage usage);
        void NewUsage(AppsTracker.Data.Models.UsageTypes usageType, int userId, int parentUsageId);
        void UsageEnded(AppsTracker.Data.Models.UsageTypes usageType);
        void EndAllUsages();
    }
}
