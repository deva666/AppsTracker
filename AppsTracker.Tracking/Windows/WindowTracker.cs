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
using AppsTracker.Common.Utils;
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
        private readonly IAppChangedNotifier appNotifierInstance;
        private readonly IScreenshotTracker screenshotTracker;
        private readonly IMediator mediator;
        private readonly IWorkQueue<Log> workQueue;

        private readonly IDictionary<Guid, LogInfo> unsavedLogsMap = new Dictionary<Guid, LogInfo>();

        private LazyInit<IAppChangedNotifier> appChangedNotifier;

        private Setting settings;
        private LogInfo activeLogInfo;

        [ImportingConstructor]
        public WindowTracker(ITrackingService trackingService,
                             IAppChangedNotifier appChangedNotifier,
                             IScreenshotTracker screenshotTracker,
                             IMediator mediator,
                             IWorkQueue<Log> workQueue)
        {
            this.trackingService = trackingService;
            this.appNotifierInstance = appChangedNotifier;
            this.screenshotTracker = screenshotTracker;
            this.mediator = mediator;
            this.workQueue = workQueue;

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

            if (settings.TrackingEnabled)
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
                        settings.TrackingEnabled;
        }

        private async void AppChanging(object sender, AppChangedArgs e)
        {
            var saveTask = EndActiveLog();

            if (!isTrackingEnabled || e.LogInfo.AppInfo == AppInfo.EmptyAppInfo ||
                (string.IsNullOrEmpty(e.LogInfo.AppInfo.Name)
                && string.IsNullOrEmpty(e.LogInfo.AppInfo.FullName)
                && string.IsNullOrEmpty(e.LogInfo.AppInfo.FileName)))
            {
                return;
            }

            activeLogInfo = e.LogInfo;
            var valueFactory = new Func<Log>(() => trackingService.CreateLogEntry(activeLogInfo));
            var log = await workQueue.EnqueueWork(valueFactory);
            if (log.LogInfoGuid != activeLogInfo.Guid || isTrackingEnabled == false)
            {
                LogInfo loginfo;
                if (unsavedLogsMap.TryGetValue(log.LogInfoGuid, out loginfo))
                {
                    loginfo.Log = log;
                    unsavedLogsMap.Remove(loginfo.Guid);
                    await trackingService.EndLogEntry(loginfo);
                }
                else
                {
                    System.Diagnostics.Debug.Fail("Failed to get loginfo from unsavedMap");
                }
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
            activeLogInfo = LogInfo.EmptyLog;
            if (logCopy.IsFinished)
                return;

            logCopy.Finish();

            if (logCopy.Log == null)
            {
                unsavedLogsMap.Add(logCopy.Guid, logCopy);
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
            //if (unsavedLogsMap.Count > 0)
            //{
            //    System.Diagnostics.Debug.Fail("Stoping tracking, unsaved logs present");
            //}
        }

        private void ResumeTracking()
        {
            isTrackingEnabled = settings.TrackingEnabled;
            if (isTrackingEnabled)
                appChangedNotifier.Component.CheckActiveApp();
        }

        public void Dispose()
        {
            appChangedNotifier.Enabled = false;
            StopTracking();
            screenshotTracker.Dispose();
            workQueue.Dispose();
        }

        public int InitializationOrder
        {
            get { return 1; }
        }
    }
}
