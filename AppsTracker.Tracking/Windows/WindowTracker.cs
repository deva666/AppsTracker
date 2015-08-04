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
        private readonly IWorkQueue workQueue;

        private readonly IDictionary<Guid, LogInfo> unsavedLogsMap = new Dictionary<Guid, LogInfo>();

        private readonly IDictionary<Guid, LogInfo> unsavedLogInfos = new Dictionary<Guid, LogInfo>();
        private readonly IDictionary<Guid, Log> createdLogs = new Dictionary<Guid, Log>();

        private LazyInit<IAppChangedNotifier> appChangedNotifier;

        private Setting settings;
        private LogInfo activeLogInfo;

        [ImportingConstructor]
        public WindowTracker(ITrackingService trackingService,
                             IAppChangedNotifier appChangedNotifier,
                             IScreenshotTracker screenshotTracker,
                             IMediator mediator,
                             IWorkQueue workQueue)
        {
            this.trackingService = trackingService;
            this.appNotifierInstance = appChangedNotifier;
            this.screenshotTracker = screenshotTracker;
            this.mediator = mediator;
            this.workQueue = workQueue;

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
            var saveTask = EndActiveLog(e.LogInfo);

            if (!isTrackingEnabled || e.LogInfo.AppInfo == AppInfo.Empty ||
                (string.IsNullOrEmpty(e.LogInfo.AppInfo.Name)
                && string.IsNullOrEmpty(e.LogInfo.AppInfo.FullName)
                && string.IsNullOrEmpty(e.LogInfo.AppInfo.FileName)))
            {
                await saveTask;
                return;
            }

            var valueFactory = new Func<Object>(() => trackingService.CreateLogEntry(activeLogInfo));
            var log = (Log)await workQueue.EnqueueWork(valueFactory);
            LogInfo loginfo;
            if (unsavedLogInfos.TryGetValue(log.LogInfoGuid, out loginfo))
            {
                unsavedLogInfos.Remove(log.LogInfoGuid);
                loginfo.Log = log;
                await trackingService.EndLogEntry(loginfo);
            }
            else
            {
                createdLogs.Add(log.LogInfoGuid, log);
            }
            await saveTask;
        }

        private void CreateLogContinuation(Task<Object> task)
        {
            var log = (Log)task.Result;
            LogInfo loginfo;
            if (unsavedLogInfos.TryGetValue(log.LogInfoGuid, out loginfo))
            {
                unsavedLogInfos.Remove(log.LogInfoGuid);
                loginfo.Log = log;
                trackingService.EndLogEntry(loginfo);
            }
            else
            {
                createdLogs.Add(log.LogInfoGuid, log);
            }
        }

        private async Task EndActiveLog(LogInfo newLogInfo)
        {
            var logInfoCopy = activeLogInfo;
            activeLogInfo = newLogInfo;
            if (logInfoCopy.IsFinished)
                return;

            logInfoCopy.Finish();

            Log log;
            if (createdLogs.TryGetValue(logInfoCopy.Guid, out log))
            {
                createdLogs.Remove(logInfoCopy.Guid);
                logInfoCopy.Log = log;
                await trackingService.EndLogEntry(logInfoCopy);
            }
            else
            {
                unsavedLogInfos.Add(logInfoCopy.Guid, logInfoCopy);
            }
        }

        private void StopTracking()
        {
            isTrackingEnabled = false;
            var dummy = EndActiveLog(LogInfo.Empty);
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
