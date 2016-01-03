#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading.Tasks;
using AppsTracker.Common.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.Data.Utils;
using AppsTracker.Tracking.Hooks;

namespace AppsTracker.Tracking
{
    [Export(typeof(ITrackingModule))]
    internal sealed class WindowTracker : ITrackingModule
    {
        private bool isTrackingEnabled;

        private readonly ITrackingService trackingService;
        private readonly IDataService dataService;
        private readonly IAppChangedNotifier appChangedNotifier;
        private readonly IScreenshotTracker screenshotTracker;
        private readonly IMediator mediator;

        private Setting settings;
        private LogInfo activeLogInfo;

        [ImportingConstructor]
        public WindowTracker(ITrackingService trackingService,
                             IDataService dataService,
                             IAppChangedNotifier appChangedNotifier,
                             IScreenshotTracker screenshotTracker,
                             IMediator mediator)
        {
            this.trackingService = trackingService;
            this.dataService = dataService;
            this.appChangedNotifier = appChangedNotifier;
            this.screenshotTracker = screenshotTracker;
            this.mediator = mediator;

            appChangedNotifier.AppChanged += OnAppChanging;
            activeLogInfo = LogInfo.Empty;
        }

        public void SettingsChanged(Setting settings)
        {
            this.settings = settings;
            ConfigureComponents();
            screenshotTracker.SettingsChanging(settings);
        }

        public void Initialize(Setting settings)
        {
            this.settings = settings;

            screenshotTracker.Initialize(settings);
            screenshotTracker.ScreenshotTaken += ScreenshotTaken;

            ConfigureComponents();

            mediator.Register(MediatorMessages.STOP_TRACKING, new Action(StopTracking));
            mediator.Register(MediatorMessages.RESUME_TRACKING, new Action(ResumeTracking));

            if (settings.TrackingEnabled)
            {
                appChangedNotifier.CheckActiveApp();
            }
        }

        private void ScreenshotTaken(object sender, ScreenshotEventArgs e)
        {
            var screenshot = e.Screenshot;

            if (isTrackingEnabled == false
                || screenshot == null
                || activeLogInfo.Guid == LogInfo.Empty.Guid)
                return;

            activeLogInfo.Screenshots.Add(screenshot);
        }

        private void ConfigureComponents()
        {
            isTrackingEnabled = settings.TrackingEnabled;
        }

        private async void OnAppChanging(Object sender, AppChangedArgs e)
        {
            if (isTrackingEnabled == false)
            {
                return;
            }

            if (e.LogInfo.AppInfo == AppInfo.Empty ||
               string.IsNullOrEmpty(e.LogInfo.AppInfo.GetAppName()))
            {
                await FinishActiveLogInfoAsync(LogInfo.Empty);
                return;
            }

            await FinishActiveLogInfoAsync(e.LogInfo);

            var log = await CreateLogAsync(e.LogInfo);

            if (!isTrackingEnabled || log.LogInfoGuid != activeLogInfo.Guid)
            {
                await FinishLogAsync(log);
            }
            else
            {
                activeLogInfo.Log = log;
            }
        }

        private async Task FinishActiveLogInfoAsync(LogInfo newLogInfo)
        {
            var logInfoCopy = activeLogInfo;
            activeLogInfo = newLogInfo;

            if (logInfoCopy.Guid != LogInfo.Empty.Guid)
            {
                await FinishLogInfoAsync(logInfoCopy);
            }
        }


        private void FinishActiveLogInfo(LogInfo newLogInfo)
        {
            var logInfoCopy = activeLogInfo;
            activeLogInfo = newLogInfo;

            if (logInfoCopy.Guid != LogInfo.Empty.Guid)
            {
                FinishLog(logInfoCopy);
            }
        }

        private async Task FinishLogInfoAsync(LogInfo logInfo)
        {
            var log = logInfo.Log;
            if (log == null)
                return;

            log.Finish();

            if (logInfo.HasScreenshots)
            {
                foreach (var screenshot in logInfo.Screenshots)
                {
                    screenshot.LogID = log.LogID;
                }
                await dataService.SaveNewEntityRangeAsync(logInfo.Screenshots);
            }

            await dataService.SaveModifiedEntityAsync(log);
        }

        private async Task FinishLogAsync(Log log)
        {
            if (log == null)
                return;

            log.Finish();
            await dataService.SaveModifiedEntityAsync(log);
        }

        private void FinishLog(LogInfo logInfo)
        {
            var log = logInfo.Log;
            if (log == null)
                return;

            log.Finish();

            if (logInfo.HasScreenshots)
            {
                foreach (var screenshot in logInfo.Screenshots)
                {
                    screenshot.LogID = log.LogID;
                }
                dataService.SaveNewEntityRange(logInfo.Screenshots);
            }

            dataService.SaveModifiedEntity(log);
        }

        private async Task<Log> CreateLogAsync(LogInfo logInfo)
        {
            var appName = logInfo.AppInfo.GetAppName();
            var appList = await dataService.GetFilteredAsync<Aplication>(a => a.UserID == trackingService.UserID
                                                                        && a.Name == appName);
            var app = appList.FirstOrDefault();
            var isNewApp = false;
            if (app == null)
            {
                app = new Aplication(logInfo.AppInfo) { UserID = trackingService.UserID };
                await dataService.SaveNewEntityAsync(app);
                isNewApp = true;
            }

            var windowsList = await dataService.GetFilteredAsync<Window>(w => w.Title == logInfo.WindowTitle
                                                                   && w.Application.ApplicationID == app.ApplicationID);
            var window = windowsList.FirstOrDefault();
            if (window == null)
            {
                window = new Window(logInfo.WindowTitle, app.ApplicationID);
                await dataService.SaveNewEntityAsync(window);
            }

            var log = new Log(window.WindowID, trackingService.UsageID, logInfo.Guid)
            {
                DateCreated = logInfo.Start,
                UtcDateCreated = logInfo.UtcStart,
                DateEnded = logInfo.End,
                UtcDateEnded = logInfo.UtcEnd,
            };

            await dataService.SaveNewEntityAsync(log);

            if (isNewApp)
                mediator.NotifyColleagues(MediatorMessages.APPLICATION_ADDED, app);

            return log;
        }

        private void StopTracking()
        {
            isTrackingEnabled = false;
            FinishActiveLogInfo(LogInfo.Empty);
        }

        private void ResumeTracking()
        {
            isTrackingEnabled = settings.TrackingEnabled;
            if (isTrackingEnabled)
            {
                appChangedNotifier.CheckActiveApp();
            }
        }

        public void Dispose()
        {
            appChangedNotifier.Dispose();
            screenshotTracker.Dispose();
            StopTracking();
        }

        public int InitializationOrder
        {
            get { return 1; }
        }
    }
}
