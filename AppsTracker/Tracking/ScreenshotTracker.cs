using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Service;
using AppsTracker.Tracking.Helpers;

namespace AppsTracker.Tracking
{
    [Export(typeof(IScreenshotTracker))]
    internal sealed class ScreenshotTracker : IScreenshotTracker
    {
        public event EventHandler<ScreenshotEventArgs> ScreenshotTaken;

        private readonly IScreenshotFactory screenshotFactory;
        private readonly ISyncContext syncContext;
        private readonly ILoggingService loggingService;

        private LazyInit<System.Timers.Timer> screenshotTimer;

        private Setting settings;

        [ImportingConstructor]
        public ScreenshotTracker(IScreenshotFactory screenshotFactory, ISyncContext syncContext
                                    , ILoggingService loggingService)
        {
            this.screenshotFactory = screenshotFactory;
            this.syncContext = syncContext;
            this.loggingService = loggingService;
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

        private void TimerTick(object sender, System.Timers.ElapsedEventArgs e)
        {
            syncContext.Invoke(async () =>
            {               
                await TakeScreenshot();
            });
        }

        private async Task TakeScreenshot()
        {
            var dbSizeTask = loggingService.GetDBSizeAsync();
            var screenshot = screenshotFactory.CreateScreenshot();

            ScreenshotTaken.InvokeSafely(this, new ScreenshotEventArgs(screenshot));

            await dbSizeTask;
        }

        public void SettingsChanging(Setting settings)
        {
            this.settings = settings;
            ConfigureComponents();
        }

        private void ConfigureComponents()
        {
            screenshotTimer.Enabled = (settings.TakeScreenshots && settings.LoggingEnabled);

            if ((settings.TakeScreenshots && settings.LoggingEnabled) && settings.TimerInterval != screenshotTimer.Component.Interval)
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
