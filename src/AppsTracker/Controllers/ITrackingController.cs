using System;
using AppsTracker.Data.Models;

namespace AppsTracker.Controllers
{
    public interface ITrackingController 
    {
        void Initialize(Setting settings);

        void SettingsChanging(Setting settings);

        void Shutdown();
    }
}
