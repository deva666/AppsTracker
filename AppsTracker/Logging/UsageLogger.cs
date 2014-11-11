using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AppsTracker.Models.Proxy;
using AppsTracker.DAL.Service;
using AppsTracker.Models.EntityModels;
using AppsTracker.MVVM;
using AppsTracker.Common.Utils;

namespace AppsTracker.Logging
{
    internal sealed class UsageLogger : IComponent, ICommunicator
    {
        private bool _isLoggingEnabled;

        private Usage _currentUsageLocked;
        private Usage _currentUsageIdle;
        private Usage _currentUsageLogin;
        private Usage _currentUsageStopped;

        private LazyInit<IdleMonitor> _idleMonitor;

        private IAppsService _service;
        private ISettings _settings;

        public UsageLogger(ISettings settings)
        {
            Ensure.NotNull(settings);

            _service = ServiceFactory.Get<IAppsService>();
            _settings = settings;

            Init();
            Configure();
        }

        private void Init()
        {
            var user = _service.InitUzer(Environment.UserName);
            _currentUsageLogin = _service.InitLogin(user.UserID);

            Globals.Initialize(user, _currentUsageLogin.UsageID);

            _idleMonitor = new LazyInit<IdleMonitor>(() => new IdleMonitor(),
                                                            m =>
                                                            {
                                                                m.IdleEntered += IdleEntered;
                                                                m.IdleStoped += IdleStopped;
                                                            },
                                                            m =>
                                                            {
                                                                m.IdleEntered -= IdleEntered;
                                                                m.IdleStoped -= IdleStopped;
                                                            })
                                                            {
                                                                Enabled = (_settings.EnableIdle && _settings.LoggingEnabled)
                                                            };

            Microsoft.Win32.SystemEvents.SessionSwitch += SessionSwitch;
        }

        private void IdleStopped(object sender, EventArgs e)
        {
            Mediator.NotifyColleagues<object>(MediatorMessages.IdleStopped);
            if (_currentUsageIdle == null)
                return;

            _currentUsageIdle.UsageEnd = DateTime.Now;
            SaveUsage(UsageTypes.Idle.ToString(), _currentUsageIdle);
            _currentUsageIdle = null;
        }

        private void IdleEntered(object sender, EventArgs e)
        {
            if (_isLoggingEnabled == false)
                return;

            _currentUsageIdle = new Usage(Globals.UserID) { SelfUsageID = Globals.UsageID };
            Mediator.NotifyColleagues<object>(MediatorMessages.IdleEntered);
        }

        private void SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (_isLoggingEnabled == false)
                return;

            if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionLock)
            {
                _currentUsageLocked = new Usage(Globals.UserID) { SelfUsageID = Globals.UsageID };
                if (_currentUsageIdle != null)
                {
                    string usageType = UsageTypes.Idle.ToString();
                    _currentUsageIdle.UsageEnd = DateTime.Now;
                    SaveUsage(usageType, _currentUsageIdle);
                    _currentUsageIdle = null;
                }
                _idleMonitor.Enabled = false;
            }
            else if (e.Reason == Microsoft.Win32.SessionSwitchReason.SessionUnlock)
            {
                _idleMonitor.Enabled = _settings.LoggingEnabled && _settings.EnableIdle;
                if (_currentUsageLocked != null)
                {
                    string usageType = UsageTypes.Locked.ToString();
                    _currentUsageLocked.UsageEnd = DateTime.Now;
                    SaveUsage(usageType, _currentUsageLocked);
                    _currentUsageLocked = null;
                }
            }
        }

        private void SaveUsage(string usageType, Usage usage)
        {
            var usageT = _service.GetSingle<UsageType>(t => t.UType == usageType);
            Ensure.NotNull(usageT, string.Concat("Can't load ", usageType));

            var usageID = usageT.UsageTypeID;
            usage.UsageTypeID = usageID;
            _service.Add<Usage>(usage);
        }

        public void SettingsChanged(ISettings settings)
        {
            _settings = settings;
            Configure();
        }

        private void Configure()
        {
           _idleMonitor.Enabled = 
               _isLoggingEnabled =
               _settings.EnableIdle && _settings.LoggingEnabled;
        }

        private void Finish()
        {
            _currentUsageLogin.UsageEnd = DateTime.Now;
            _currentUsageLogin.IsCurrent = false;
            SaveUsage(UsageTypes.Login.ToString(), _currentUsageLogin);
        }

        public void Dispose()
        {
            _idleMonitor.Enabled = false;
            Finish();
            Microsoft.Win32.SystemEvents.SessionSwitch -= SessionSwitch;
        }

        public IMediator Mediator
        {
            get { return MVVM.Mediator.Instance; }
        }


        public void SetComponentEnabled(bool enabled)
        {
            _isLoggingEnabled = enabled;
        }
    }
}
