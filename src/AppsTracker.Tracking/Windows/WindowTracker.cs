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
using System.Linq;
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
        private readonly IAppChangedNotifier appChangedNotifier;
        private readonly IScreenshotTracker screenshotTracker;
        private readonly IMediator mediator;
        private readonly IWorkQueue workQueue;

        private readonly IDictionary<Guid, LogInfo> unsavedLogInfos = new Dictionary<Guid, LogInfo>();
        private readonly IDictionary<Guid, Log> createdLogs = new Dictionary<Guid, Log>();

        private Setting settings;
        private LogInfo activeLogInfo;

        [ImportingConstructor]
        public WindowTracker(IAppChangedNotifier appChangedNotifier,
                             ITrackingService trackingService,
                             IScreenshotTracker screenshotTracker,
                             IMediator mediator,
                             IWorkQueue workQueue)
        {
            this.trackingService = trackingService;
            this.appChangedNotifier = appChangedNotifier;
            this.screenshotTracker = screenshotTracker;
            this.mediator = mediator;
            this.workQueue = workQueue;

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
                appChangedNotifier.CheckActiveApp();
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
            isTrackingEnabled = settings.TrackingEnabled;
        }

        private void OnAppChanging(Object sender, AppChangedArgs e)
        {
            if (isTrackingEnabled == false)
            {
                return;
            }

            if (e.LogInfo.AppInfo == AppInfo.Empty ||
               (string.IsNullOrEmpty(e.LogInfo.AppInfo.Name)
               && string.IsNullOrEmpty(e.LogInfo.AppInfo.FullName)
               && string.IsNullOrEmpty(e.LogInfo.AppInfo.FileName)))
            {
                SwapLogs(LogInfo.Empty);
                return;
            }
            var valueFactory = new Func<Object>(() => trackingService.CreateLogEntry(e.LogInfo));
            var createTask = workQueue.EnqueueWork(valueFactory);
            createTask.ContinueWith(OnLogCreated, TaskScheduler.FromCurrentSynchronizationContext());
            SwapLogs(e.LogInfo);
        }

        private void SwapLogs(LogInfo logInfo)
        {
            if (activeLogInfo.IsFinished == false)
            {
                activeLogInfo.Finish();
                Log log;
                if (createdLogs.TryGetValue(activeLogInfo.Guid, out log))
                {
                    var logCopy = activeLogInfo;
                    createdLogs.Remove(activeLogInfo.Guid);
                    logCopy.Log = log;
                    workQueue.EnqueueWork(() => trackingService.EndLogEntry(logCopy));
                }
                else
                {
                    unsavedLogInfos.Add(activeLogInfo.Guid, activeLogInfo);
                }
            }
            activeLogInfo = logInfo;
        }


        private void OnLogCreated(Task<Object> task)
        {
            var log = (Log)task.Result;

            if (isTrackingEnabled == false)
            {
                workQueue.EnqueueWork(() => trackingService.EndLogEntry(log));
                if (unsavedLogInfos.ContainsKey(log.LogInfoGuid))
                    unsavedLogInfos.Remove(log.LogInfoGuid);
                return;
            }

            LogInfo loginfo;
            if (unsavedLogInfos.TryGetValue(log.LogInfoGuid, out loginfo))
            {
                unsavedLogInfos.Remove(log.LogInfoGuid);
                loginfo.Log = log;
                workQueue.EnqueueWork(() => trackingService.EndLogEntry(loginfo));
            }
            else
            {
                createdLogs.Add(log.LogInfoGuid, log);
            }
        }

        private void StopTracking()
        {
            isTrackingEnabled = false;
            SwapLogs(LogInfo.Empty);

            if (createdLogs.Count > 0)
            {
                foreach (var pair in createdLogs.ToList())
                {
                    trackingService.EndLogEntry(pair.Value);
                    createdLogs.Remove(pair.Key);
                }
            }

            if (unsavedLogInfos.Count > 0)
            {
                foreach (var pair in unsavedLogInfos.ToList())
                {
                    unsavedLogInfos.Remove(pair.Key);
                }
            }
#if DEBUG
            if (createdLogs.Count > 0)
                throw new InvalidProgramException("Created logs count > 0");

            if (unsavedLogInfos.Count > 0)
                throw new InvalidProgramException("Unsaved log infos count > 0");
#endif
        }

        private void ResumeTracking()
        {
            isTrackingEnabled = settings.TrackingEnabled;
            if (isTrackingEnabled)
                appChangedNotifier.CheckActiveApp();
        }

        public void Dispose()
        {
            appChangedNotifier.Dispose();
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
