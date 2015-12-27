using System;
using System.Threading.Tasks;

namespace AppsTracker.Tracking.Helpers
{
    public interface ILogCleaner : IDisposable
    {
        Task Clean();
        
        int Days { get; set; }
    }
}
