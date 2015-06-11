using System;
namespace AppsTracker.Tracking.Helpers
{
    internal interface IUsageProcessor
    {
        void NewUsage(AppsTracker.Data.Models.UsageTypes usageType, int userId, int parentUsageId);
        void UsageEnded(AppsTracker.Data.Models.UsageTypes usageType);
        void EndAllUsages();
    }
}
