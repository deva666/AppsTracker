#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using AppsTracker.Common.Communication;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;
using AppsTracker.Data.Utils;
using AppsTracker.Tracking.Helpers;
using AppsTracker.Tracking.Hooks;

namespace AppsTracker.Tracking
{
    [Export(typeof(ITrackingModule))]
    internal sealed class WindowTracker : ITrackingModule
    {
        private bool isTrackingEnabled;

        private readonly ITrackingService trackingService;
        private readonly IDataService dataService;
        private readonly IAppChangedNotifier appNotifierInstance;
        private readonly ISyncContext syncContext;
        private readonly IScreenshotTracker screenshotTracker;
        private readonly IMediator mediator;

        private readonly IDictionary<Guid, LogInfo> unsavedLogsMap = new Dictionary<Guid, LogInfo>();

        private LazyInit<IAppChangedNotifier> appChangedNotifier;

        private Setting settings;
        private LogInfo activeLogInfo;

        [ImportingConstructor]
        public WindowTracker(ITrackingService trackingService,
                             IDataService dataService,
                             IAppChangedNotifier appChangedNotifier,
                             IScreenshotTracker screenshotTracker,
                             ISyncContext syncContext,
                             IMediator mediator)
        {
            this.trackingService = trackingService;
            this.dataService = dataService;
            this.appNotifierInstance = appChangedNotifier;
            this.screenshotTracker = screenshotTracker;
            this.syncContext = syncContext;
            this.mediator = mediator;

            activeLogInfo = LogInfo.EmptyLog;
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

            appChangedNotifier = new LazyInit<IAppChangedNotifier>(() => appNotifierInstance,
                                                           a => a.AppChanged += AppChanging,
                                                           a => a.AppChanged -= AppChanging);

            ConfigureComponents();

            mediator.Register(MediatorMessages.STOP_TRACKING, new Action(StopTracking));
            mediator.Register(MediatorMessages.RESUME_TRACKING, new Action(ResumeTracking));

            if (settings.LoggingEnabled)
                appChangedNotifier.Component.CheckActiveApp();
        }

        private void ScreenshotTaken(object sender, ScreenshotEventArgs e)
        {
            var screenshot = e.Screenshot;

            if (isTrackingEnabled == false || screenshot == null)
                return;

            activeLogInfo.Screenshots.Add(screenshot);
        }

        private void ConfigureComponents()
        {
            isTrackingEnabled =
                appChangedNotifier.Enabled =
                        settings.LoggingEnabled;
        }

        private async void AppChanging(object sender, AppChangedArgs e)
        {
            var saveTask = EndActiveLog();

            if (!isTrackingEnabled || e.AppInfo == null ||
                (e.AppInfo != null
                && string.IsNullOrEmpty(e.AppInfo.Name)
                && string.IsNullOrEmpty(e.AppInfo.FullName)
                && string.IsNullOrEmpty(e.AppInfo.FileName)))
            {
                return;
            }

            activeLogInfo = new LogInfo(e.AppInfo, e.WindowTitle);
            var log = await trackingService.CreateLogEntryAsync(activeLogInfo);
            if (log.LogInfoGuid != activeLogInfo.Guid || isTrackingEnabled == false)
            {
                System.Diagnostics.Debug.WriteLine("Unsaved log try get out");
                LogInfo loginfo;
                if (!unsavedLogsMap.TryGetValue(log.LogInfoGuid, out loginfo))
                {
                    System.Diagnostics.Debug.WriteLine("Failed try get out, loginfo not in unsaved map");
                }
                loginfo.Log = log;
                unsavedLogsMap.Remove(loginfo.Guid);
                await trackingService.EndLogEntry(loginfo);
            }
            else
            {
                activeLogInfo.Log = log;
            }

            await saveTask;
        }

        private async Task EndActiveLog()
        {
            var logCopy = activeLogInfo;
            if (logCopy.IsFinished)
                return;

            logCopy.Finish();

            if (logCopy.Log == null)
            {
                unsavedLogsMap.Add(logCopy.Guid, logCopy);
                System.Diagnostics.Debug.WriteLine("Adding log to unsaved map");
                return;
            }
            else
            {
                await trackingService.EndLogEntry(logCopy);
            }
        }

        private void StopTracking()
        {
            isTrackingEnabled = false;
            var dummy = EndActiveLog();
            if (unsavedLogsMap.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine("Stoping tracking, unsaved logs present");
            }
            //unsavedLogsMap.Values.ForEachAction(async l => await EndLog(l));
        }

        private void ResumeTracking()
        {
            isTrackingEnabled = settings.LoggingEnabled;
        }

        public void Dispose()
        {
            appChangedNotifier.Enabled = false;
            StopTracking();
            screenshotTracker.Dispose();
        }


        public int InitializationOrder
        {
            get { return 1; }
        }

    }
}
