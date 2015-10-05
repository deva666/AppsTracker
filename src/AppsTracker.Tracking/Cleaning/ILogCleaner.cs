using System;

namespace AppsTracker.Tracking.Helpers
{
    public interface ILogCleaner : IDisposable
    {
        void Clean();
        
        int Days { get; set; }
    }
}
