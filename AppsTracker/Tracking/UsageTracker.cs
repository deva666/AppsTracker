#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using AppsTracker.Data.Models;
using AppsTracker.Service;
using AppsTracker.ServiceLocation;
using AppsTracker.MVVM;

namespace AppsTracker.Tracking
{
    [Export(typeof(IModule))]
    internal sealed class UsageTracker : IModule
    {
        private bool isLoggingEnabled;

        private readonly ITrackingService trackingService;
        private readonly IDataService dataService;
        private readonly IMediator mediator;
        private readonly IIdleNotifier idleNotifierInstance;

        private LazyInit<IIdleNotifier> idleNotifier;

        private Usage currentUsageLocked;
        private Usage currentUsageIdle;
        private Usage currentUsageLogin;
        private Usage currentUsageStopped;

        private Setting settings;

        [ImportingConstructor]
        public UsageTracker(IIdleNotifier idleNotifierInstance,
                            ITrackingService trackingService,
                            IDataService dataService,
                            IMediator mediator)
        {
            this.idleNotifierInstance = idleNotifierInstance;
            this.trackingService = trackingService;
            this.dataService = dataService;
            this.mediator = mediator;
        }


        public void InitializeComponent(Setting settings)
        {
            this.settings = settings;

            idleNotifier = new LazyInit<IIdleNotifier>(() => idleNotifierInstance, OnIdleNotifierInit, OnIdleNotifierDispose);

            InitLogin();

            Microsoft.Win32.SystemEvents.SessionSwitch += SessionSwitch;
            Microsoft.Win32.SystemEvents.PowerModeChanged += PowerModeChanged;

            Configure();
        }

        private void OnIdleNotifierInit(IIdleNotifier notifier)
        {
            notifier.IdleEntered += IdleEntered;
            notifier.IdleStoped += IdleStopped;
        }

        private void OnIdleNotifierDispose(IIdleNotifier notifier)
        {
            notifier.IdleEntered -= IdleEntered;
            notifier.IdleStoped -= IdleStopped;
        }

        private void Configure()
        {
            idleNotifier.Enabled = settings.LoggingEnabled && settings.EnableIdle;
            isLoggingEnabled = settings.LoggingEnabled;
            CheckStoppedUsage();
        }

        private void InitLogin()
        {
            var user = trackingService.GetUzer(Environment.UserName);
            currentUsageLogin = trackingService.LoginUser(user.UserID);

            trackingService.Initialize(user, currentUsageLogin.UsageID);
        }


        private void IdleStopped(object sender, EventArgs e)
        {
            mediator.NotifyColleagues(MediatorMessages.RESUME_LOGGING);
            if (currentUsageIdle == null)
                return;

            currentUsageIdle.UsageEnd = DateTime.Now;
            AddUsage(UsageTypes.Idle, currentUsageIdle);
            currentUsageIdle = null;
        }


        private void IdleEntered(object sender, EventArgs e)
        {
            if (isLoggingEnabled == false)
                return;

            currentUsageIdle = new Usage(trackingService.UserID) { SelfUsageID = trackingService.UsageID };
            mediator.NotifyColleagues(MediatorMessages.STOP_LOGGING);
        }


        private void PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case Microsoft.Win32.PowerModes.Resume:
                    InitLogin();
                    Configure();
                    mediator.NotifyColleagues(MediatorMessages.RESUME_LOGGING);
                    Microsoft.Win32.SystemEvents.SessionSwitch += SessionSwitch;
                    break;
                case Microsoft.Win32.PowerModes.StatusChange:
                    break;
                case Microsoft.Win32.PowerModes.Suspend:
                    //Session Switch event is fired immediately after the computer is being put to sleep,
                    //If it's going to sleep, then don't log this as computer locked
                    isLoggingEnabled = false;
                    Microsoft.Win32.SystemEvents.SessionSwitch -= SessionSwitch;
                    mediator.NotifyColleagues(MediatorMessages.STOP_LOGGING);
                    Finish();
                    break;
            }
        }


        private void SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (isLoggingEnabled == false)
                return;

            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
            {
                currentUsageLocked = new Usage(trackingService.UserID) { SelfUsageID = trackingService.UsageID };
                if (currentUsageIdle != null)
                {
                    currentUsageIdle.UsageEnd = DateTime.Now;
                    AddUsage(UsageTypes.Idle, currentUsageIdle);
                    currentUsageIdle = null;
                }
                idleNotifier.Enabled = false;
            }
            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
            {
                if (settings.EnableIdle && settings.LoggingEnabled)
                {
                    idleNotifier.Enabled = true;
                }
                if (currentUsageLocked != null)
                {
                    currentUsageLocked.UsageEnd = DateTime.Now;
                    AddUsage(UsageTypes.Locked, currentUsageLocked);
                    currentUsageLocked = null;
                }
            }
        }


        private void AddUsage(UsageTypes usagetype, Usage usage)
        {
            usage.UsageType = usagetype;
            dataService.SaveNewEntity(usage);
        }


        private void SaveUsage(Usage usage)
        {
            if (usage == null)
                return;

            usage.UsageEnd = DateTime.Now;
            dataService.SaveModifiedEntity(usage);
        }


        public void SettingsChanged(Setting settings)
        {
            this.settings = settings;
            Configure();
        }

        private void CheckStoppedUsage()
        {
            if (isLoggingEnabled == false && currentUsageStopped == null)
                currentUsageStopped = new Usage(trackingService.UserID) { SelfUsageID = trackingService.UsageID };
            else if (isLoggingEnabled && currentUsageStopped != null)
                SaveStoppedUsage();
        }


        private void SaveStoppedUsage()
        {
            if (currentUsageStopped != null)
            {
                currentUsageStopped.UsageEnd = DateTime.Now;
                AddUsage(UsageTypes.Stopped, currentUsageStopped);
                currentUsageStopped = null;
            }
        }


        private void Finish()
        {
            currentUsageLogin.IsCurrent = false;
            SaveUsage(currentUsageIdle);
            SaveUsage(currentUsageLocked);
            SaveUsage(currentUsageLogin);
            SaveStoppedUsage();
        }


        public void Dispose()
        {
            idleNotifierInstance.Dispose();
            idleNotifier.Enabled = false;
            Finish();
            Microsoft.Win32.SystemEvents.SessionSwitch -= SessionSwitch;
            Microsoft.Win32.SystemEvents.PowerModeChanged -= PowerModeChanged;
        }
    }
}
