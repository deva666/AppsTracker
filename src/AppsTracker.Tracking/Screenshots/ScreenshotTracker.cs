using System;
using System.ComponentModel.Composition;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using AppsTracker.Common.Utils;
using AppsTracker.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Repository;
using AppsTracker.Tracking.Helpers;

namespace AppsTracker.Tracking
{
    [Export(typeof(IScreenshotTracker))]
    internal sealed class ScreenshotTracker : IScreenshotTracker
    {
        private readonly IScreenshotFactory screenshotFactory;
        private readonly ISyncContext syncContext;
        private readonly IRepository repository;

        private readonly Subject<Screenshot> subject = new Subject<Screenshot>();

        private LazyInit<System.Timers.Timer> screenshotTimer;

        private Setting settings;

        public IObservable<Screenshot> ScreenshotObservable
        {
            get { return subject.AsObservable(); }
        }

        [ImportingConstructor]
        public ScreenshotTracker(IScreenshotFactory screenshotFactory,
                                 ISyncContext syncContext,
                                 IRepository repository)
        {
            this.screenshotFactory = screenshotFactory;
            this.syncContext = syncContext;
            this.repository = repository;
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
            var dbSizeTask = repository.GetDBSizeAsync();
            var screenshot = screenshotFactory.CreateScreenshot();

            await dbSizeTask;

            subject.OnNext(screenshot);
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
}
