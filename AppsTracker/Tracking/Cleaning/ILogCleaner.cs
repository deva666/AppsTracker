using System;

namespace AppsTracker.Tracking.Helpers
{
    interface ILogCleaner : IDisposable
    {
        void Clean();
        System.Threading.Tasks.Task CleanAsync();
        int Days { get; set; }
    }
}
