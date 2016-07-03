using System;
using System.Reactive;
using AppsTracker.Data.Models;

namespace AppsTracker.Tracking
{
    public interface IScreenshotTracker : IDisposable
    {
        IObservable<Screenshot> ScreenshotObservable
        {
            get;
        }

        void SettingsChanging(Setting settings);

        void Initialize(Setting settings);
    }
}
