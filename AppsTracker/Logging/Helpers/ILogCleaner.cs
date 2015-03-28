using System;

namespace AppsTracker.Logging.Helpers
{
    interface ILogCleaner : IDisposable
    {
        void Clean();
        System.Threading.Tasks.Task CleanAsync();
        int Days { get; set; }
    }
}
