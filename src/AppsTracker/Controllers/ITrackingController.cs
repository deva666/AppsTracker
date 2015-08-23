using System;
using AppsTracker.Data.Models;

namespace AppsTracker.Controllers
{
    public interface ITrackingController : IDisposable
    {
        void Initialize(Setting settings);
        void SettingsChanging(Setting settings);
    }
}
