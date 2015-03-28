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
using AppsTracker.Data.Service;
using AppsTracker.MVVM;

namespace AppsTracker.Logging
{
    [Export(typeof(IComponent))]
    internal sealed class UsageLogger : IComponent, ICommunicator
    {
        private IIdleNotifier idleNotifierInstance;

        private bool isLoggingEnabled;

        private readonly ILoggingService loggingService;

        private LazyInit<IIdleNotifier> idleNotifier;

        private Usage currentUsageLocked;
        private Usage currentUsageIdle;
        private Usage currentUsageLogin;
        private Usage currentUsageStopped;

        private Setting settings;

        [ImportingConstructor]
        public UsageLogger(IIdleNotifier idleNotifier)
        {
            loggingService = ServiceFactory.Get<ILoggingService>();
            idleNotifierInstance = idleNotifier;
        }


        public void InitializeComponent(Setting settings)
        {
            this.settings = settings;

            idleNotifier = new LazyInit<IIdleNotifier>(() => idleNotifierInstance,
                                                        i =>
                                                        {
                                                            i.IdleEntered += IdleEntered;
                                                            i.IdleStoped += IdleStopped;
                                                        },
                                                        i =>
                                                        {
                                                            i.IdleEntered -= IdleEntered;
                                                            i.IdleStoped -= IdleStopped;
                                                        });

            InitLogin();

            Microsoft.Win32.SystemEvents.SessionSwitch += SessionSwitch;
            Microsoft.Win32.SystemEvents.PowerModeChanged += PowerModeChanged;

            Configure();
        }


        private void Configure()
        {
            idleNotifier.Enabled = settings.LoggingEnabled && settings.EnableIdle;
            isLoggingEnabled = settings.LoggingEnabled;
            CheckStoppedUsage();
        }

        private void InitLogin()
        {
            var user = GetUzer(Environment.UserName);
            currentUsageLogin = LoginUser(user.UserID);

            Globals.Initialize(user, currentUsageLogin.UsageID);
        }

        private Usage LoginUser(int userID)
        {
            return loggingService.LoginUser(userID);
        }

        private Uzer GetUzer(string userName)
        {
            return loggingService.GetUzer(userName);
        }

        private void IdleStopped(object sender, EventArgs e)
        {
            Mediator.NotifyColleagues(MediatorMessages.RESUME_LOGGING);
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

            currentUsageIdle = new Usage(Globals.UserID) { SelfUsageID = Globals.UsageID };
            Mediator.NotifyColleagues(MediatorMessages.STOP_LOGGING);
        }


        private void PowerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case Microsoft.Win32.PowerModes.Resume:
                    InitLogin();
                    Configure();
                    Mediator.NotifyColleagues(MediatorMessages.RESUME_LOGGING);
                    Microsoft.Win32.SystemEvents.SessionSwitch += SessionSwitch;
                    break;
                case Microsoft.Win32.PowerModes.StatusChange:
                    break;
                case Microsoft.Win32.PowerModes.Suspend:
                    //Session Switch event is fired immediately after the computer is being put to sleep,
                    //If it's going to sleep, then don't log this as computer locked
                    isLoggingEnabled = false;
                    Microsoft.Win32.SystemEvents.SessionSwitch -= SessionSwitch;
                    Mediator.NotifyColleagues(MediatorMessages.STOP_LOGGING);
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
                currentUsageLocked = new Usage(Globals.UserID) { SelfUsageID = Globals.UsageID };
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
            loggingService.SaveNewUsageAsync(usagetype, usage);
        }


        private void SaveUsage(Usage usage)
        {
            if (usage == null)
                return;

            usage.UsageEnd = DateTime.Now;
            loggingService.SaveModifiedUsageAsync(usage);
        }


        public void SettingsChanged(Setting settings)
        {
            this.settings = settings;
            Configure();
        }

        private void CheckStoppedUsage()
        {
            if (isLoggingEnabled == false && currentUsageStopped == null)
                currentUsageStopped = new Usage(Globals.UserID) { SelfUsageID = Globals.UsageID };
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


        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }
    }
}
