using AppsTracker.Data.Models;

namespace AppsTracker.Tracking.Helpers
{
    public interface ILimitHandler
    {
        void Handle(AppLimit warning);
    }
}
