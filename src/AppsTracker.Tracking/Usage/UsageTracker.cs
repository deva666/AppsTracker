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
        private readonly ExportFactory<IIdleNotifier> idleNotifierFactory;

        private LazyInit<IIdleNotifier> idleNotifier;

        private Setting settings;

        [ImportingConstructor]
        public UsageTracker(ExportFactory<IIdleNotifier> idleNotifierFactory,
                            ITrackingService trackingService,
                            IUsageProcessor usageProcessor,
                            IMediator mediator)
        {
            this.idleNotifierFactory = idleNotifierFactory;
            this.trackingService = trackingService;
            this.usageProcessor = usageProcessor;
            this.mediator = mediator;
        }


        public void Initialize(Setting settings)
        {
            this.settings = settings;

            idleNotifier = new LazyInit<IIdleNotifier>(CreateIdleNotifiier,
                                                       OnIdleNotifierInit,
                                                       OnIdleNotifierDispose);

            InitLogin();

            Microsoft.Win32.SystemEvents.SessionSwitch += SessionSwitch;
            Microsoft.Win32.SystemEvents.PowerModeChanged += PowerModeChanged;

            mediator.Register<Boolean>(MediatorMessages.TRACKING_ENABLED_CHANGING, TrackingEnabledChanging);

            Configure();
            if (settings.TrackingEnabled == false)
                TrackingEnabledChanging(settings.TrackingEnabled);
        }

        private void TrackingEnabledChanging(Boolean enabled)
        {
            isTrackingEnabled = enabled;
            if (enabled)
            {
                usageProcessor.UsageEnded(UsageTypes.Stopped);
            }
            else
            {
                usageProcessor.EndAllUsages();
                usageProcessor.NewUsage(UsageTypes.Stopped, trackingService.UserID, trackingService.UsageID);
            }
        }

        private IIdleNotifier CreateIdleNotifiier()
        {
            var context = idleNotifierFactory.CreateExport();
            return context.Value;
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
            idleNotifier.Enabled = settings.TrackingEnabled && settings.EnableIdle;
            isTrackingEnabled = settings.TrackingEnabled;
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

            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
            {
                if (isTrackingEnabled == false)
                    return;
                usageProcessor.NewUsage(UsageTypes.Locked, trackingService.UserID, trackingService.UsageID);
                usageProcessor.UsageEnded(UsageTypes.Idle);
                mediator.NotifyColleagues(MediatorMessages.STOP_TRACKING);
                isTrackingEnabled = false;
            }
            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
            {
                if (settings.EnableIdle && settings.TrackingEnabled)
                {
                    isTrackingEnabled = true;
                }
                usageProcessor.UsageEnded(UsageTypes.Locked);
                mediator.NotifyColleagues(MediatorMessages.RESUME_TRACKING);
            }
        }


        public void SettingsChanged(Setting settings)
        {
            this.settings = settings;
            Configure();
        }


        public void Dispose()
        {
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
