using System;
using AppsTracker.Data.Models;

namespace AppsTracker.Controllers
{
    public interface ILoggingController : IDisposable
    {
        void Initialize(Setting settings);
        void SettingsChanging(Setting settings);
    }
}
