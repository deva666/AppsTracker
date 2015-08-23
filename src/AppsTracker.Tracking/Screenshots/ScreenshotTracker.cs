using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.Tracking.Helpers;
using AppsTracker.Common.Utils;
using AppsTracker.Communication;

namespace AppsTracker.Tracking
{
    [Export(typeof(IScreenshotTracker))]
    internal sealed class ScreenshotTracker : IScreenshotTracker
    {
        public event EventHandler<ScreenshotEventArgs> ScreenshotTaken;

        private readonly IScreenshotFactory screenshotFactory;
        private readonly ISyncContext syncContext;
        private readonly IDataService dataService;

        private LazyInit<System.Timers.Timer> screenshotTimer;

        private Setting settings;

        [ImportingConstructor]
        public ScreenshotTracker(IScreenshotFactory screenshotFactory,
                                 ISyncContext syncContext,
                                 IDataService dataService)
        {
            this.screenshotFactory = screenshotFactory;
            this.syncContext = syncContext;
            this.dataService = dataService;
        }

        public void Initialize(Setting settings)
        {
            this.settings = settings;

            screenshotTimer = new LazyInit<System.Timers.Timer>(() => new System.Timers.Timer()
            {
                AutoReset = true,
                Interval = settings.TimerInterval
            },
             OnScreenshotInit,
             OnScreenshotDispose);

            ConfigureComponents();
        }

        private void OnScreenshotInit(System.Timers.Timer timer)
        {
            timer.Enabled = true;
            timer.Elapsed += TimerTick;
        }

        private void OnScreenshotDispose(System.Timers.Timer timer)
        {
            timer.Enabled = false;
            timer.Elapsed -= TimerTick;
        }

        private async void TimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            var dbSizeTask = dataService.GetDBSizeAsync();
            var screenshot = screenshotFactory.CreateScreenshot();

            await dbSizeTask;

            syncContext.Invoke(() =>
            {
                ScreenshotTaken.InvokeSafely(this, new ScreenshotEventArgs(screenshot));
            });
        }

        public void SettingsChanging(Setting settings)
        {
            this.settings = settings;
            ConfigureComponents();
        }

        private void ConfigureComponents()
        {
            screenshotTimer.Enabled = (settings.TakeScreenshots && settings.TrackingEnabled);

            if ((settings.TakeScreenshots && settings.TrackingEnabled) 
                && settings.TimerInterval != screenshotTimer.Component.Interval)
                screenshotTimer.Component.Interval = settings.TimerInterval;
        }


        public void Dispose()
        {
            screenshotTimer.Enabled = false;
        }
    }

    public class ScreenshotEventArgs : EventArgs
    {
        public Screenshot Screenshot { get; private set; }

        public ScreenshotEventArgs(Screenshot screenshot)
        {
            this.Screenshot = screenshot;
        }
    }
}
