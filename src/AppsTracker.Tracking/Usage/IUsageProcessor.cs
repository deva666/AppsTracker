using AppsTracker.Data.Models;

namespace AppsTracker.Tracking.Helpers
{
    internal interface IUsageProcessor
    {
        Usage LoginUser(int userId);

        void NewUsage(UsageTypes usageType);

        void UsageEnded(UsageTypes usageType);

        void EndAllUsages();
    }
}
