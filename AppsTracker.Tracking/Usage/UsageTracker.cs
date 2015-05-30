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
using AppsTracker.Common.Communication;
using AppsTracker.Data.Service;
using AppsTracker.Tracking.Helpers;

namespace AppsTracker.Tracking
{
    [Export(typeof(ITrackingModule))]
    internal sealed class UsageTracker : ITrackingModule
    {
        private bool isTrackingEnabled;

        private readonly ITrackingService trackingService;
        private readonly IMediator mediator;
        private readonly IUsageProcessor usageProcessor;
        private readonly IIdleNotifier idleNotifierInstance;

        private LazyInit<IIdleNotifier> idleNotifier;

        private Setting settings;

        [ImportingConstructor]
        public UsageTracker(IIdleNotifier idleNotifierInstance,
                            ITrackingService trackingService,
                            IUsageProcessor usageProcessor,
                            IMediator mediator)
        {
            this.idleNotifierInstance = idleNotifierInstance;
            this.trackingService = trackingService;
            this.usageProcessor = usageProcessor;
            this.mediator = mediator;
        }


        public void Initialize(Setting settings)
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


        private void InitLogin()
        {
            var user = trackingService.GetUzer(Environment.UserName);
            var usageLogin = trackingService.LoginUser(user.UserID);

            trackingService.Initialize(user, usageLogin.UsageID);
        }


        private void Configure()
        {
            idleNotifier.Enabled = settings.LoggingEnabled && settings.EnableIdle;
            isTrackingEnabled = settings.LoggingEnabled;
            CheckStoppedUsage();
        }


        private void IdleStopped(object sender, EventArgs e)
        {
            mediator.NotifyColleagues(MediatorMessages.RESUME_TRACKING);
            usageProcessor.UsageEnded(UsageTypes.Idle);
        }


        private void IdleEntered(object sender, EventArgs e)
        {
            if (isTrackingEnabled == false)
                return;

            usageProcessor.NewUsage(UsageTypes.Idle, trackingService.UserID, trackingService.UsageID);
            mediator.NotifyColleagues(MediatorMessages.STOP_TRACKING);
        }


        private void PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case Microsoft.Win32.PowerModes.Resume:
                    InitLogin();
                    Configure();
                    mediator.NotifyColleagues(MediatorMessages.RESUME_TRACKING);
                    break;
                case Microsoft.Win32.PowerModes.StatusChange:
                    break;
                case Microsoft.Win32.PowerModes.Suspend:
                    isTrackingEnabled = false;
                    usageProcessor.EndAllUsages();
                    mediator.NotifyColleagues(MediatorMessages.STOP_TRACKING);
                    break;
            }
        }


        private void SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (isTrackingEnabled == false)
                return;

            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
            {
                usageProcessor.NewUsage(UsageTypes.Locked, trackingService.UserID, trackingService.UsageID);
                usageProcessor.UsageEnded(UsageTypes.Idle);
                idleNotifier.Enabled = false;
            }
            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
            {
                if (settings.EnableIdle && settings.LoggingEnabled)
                {
                    idleNotifier.Enabled = true;
                }
                usageProcessor.UsageEnded(UsageTypes.Locked);
            }
        }


        public void SettingsChanged(Setting settings)
        {
            this.settings = settings;
            Configure();
        }

        private void CheckStoppedUsage()
        {
            if (isTrackingEnabled == false)
                usageProcessor.NewUsage(UsageTypes.Stopped, trackingService.UserID, trackingService.UsageID);
            else if (isTrackingEnabled)
                usageProcessor.UsageEnded(UsageTypes.Stopped);
        }


        public void Dispose()
        {
            idleNotifierInstance.Dispose();
            idleNotifier.Enabled = false;
            usageProcessor.EndAllUsages();
            Microsoft.Win32.SystemEvents.SessionSwitch -= SessionSwitch;
            Microsoft.Win32.SystemEvents.PowerModeChanged -= PowerModeChanged;
        }


        public int InitializationOrder
        {
            get { return 0; }
        }
    }
}
