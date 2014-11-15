#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Data.Entity;
using System.Linq;

using AppsTracker.Common.Utils;
using AppsTracker.DAL;
using AppsTracker.DAL.Service;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.Proxy;
using AppsTracker.MVVM;

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

        private ISettings _settings;

        public UsageLogger(ISettings settings)
        {
            Ensure.NotNull(settings);

            _settings = settings;

            Init();
            Configure();
        }

        private void Init()
        {
            var user = InitUzer(Environment.UserName);
            _currentUsageLogin = InitLogin(user.UserID);

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
                                                            });

            Microsoft.Win32.SystemEvents.SessionSwitch += SessionSwitch;
        }

        private void IdleStopped(object sender, EventArgs e)
        {
            Mediator.NotifyColleagues<object>(MediatorMessages.IdleStopped);
            if (_currentUsageIdle == null)
                return;

            _currentUsageIdle.UsageEnd = DateTime.Now;
            AddUsage(UsageTypes.Idle.ToString(), _currentUsageIdle);
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
                    AddUsage(usageType, _currentUsageIdle);
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
                    AddUsage(usageType, _currentUsageLocked);
                    _currentUsageLocked = null;
                }
            }
        }

        private void AddUsage(string usagetype, Usage usage)
        {
            using (var context =new AppsEntities())
            {
                var typeID = context.UsageTypes.First(t => t.UType == usagetype).UsageTypeID;
                usage.UsageTypeID = typeID;
                context.Usages.Add(usage);
                context.SaveChanges();
            }
        }

        private void SaveUsage(Usage usage)
        {
            using (var context = new AppsEntities())
            {
                context.Entry<Usage>(usage).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public void SettingsChanged(ISettings settings)
        {
            _settings = settings;
            Configure();
        }

        private void Configure()
        {
            _idleMonitor.Enabled = _settings.EnableIdle && _settings.LoggingEnabled;
            _isLoggingEnabled = _settings.LoggingEnabled;
            CheckStoppedUsage();
        }

        private void CheckStoppedUsage()
        {
            if (_isLoggingEnabled == false && _currentUsageStopped == null)
                _currentUsageStopped = new Usage(Globals.UserID) { SelfUsageID = Globals.UsageID };
            else if (_isLoggingEnabled && _currentUsageStopped != null)
                SaveStoppedUsage();
        }

        private void SaveStoppedUsage()
        {
            if (_currentUsageStopped != null)
            {
                _currentUsageStopped.UsageEnd = DateTime.Now;
                AddUsage(UsageTypes.Stopped.ToString(), _currentUsageStopped);
                _currentUsageStopped = null;
            }
        }

        private void Finish()
        {
            _currentUsageLogin.UsageEnd = DateTime.Now;
            _currentUsageLogin.IsCurrent = false;
            SaveUsage(_currentUsageLogin);
            SaveStoppedUsage();
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

        private Usage InitLogin(int userID)
        {
            using (var context = new AppsEntities())
            {
                string loginUsage = UsageTypes.Login.ToString();

                if (context.Usages.Where(u => u.IsCurrent && u.UsageType.UType == loginUsage).Count() > 0)
                {
                    var failedSaveUsage = context.Usages.Where(u => u.IsCurrent && u.UsageType.UType == loginUsage).ToList();
                    foreach (var usage in failedSaveUsage)
                    {
                        var lastLog = context.Logs.Where(l => l.UsageID == usage.UsageID).OrderByDescending(l => l.DateCreated).FirstOrDefault();
                        var lastUsage = context.Usages.Where(u => u.SelfUsageID == usage.UsageID).OrderByDescending(u => u.UsageEnd).FirstOrDefault();

                        DateTime lastLogDate = DateTime.MinValue;
                        DateTime lastUsageDate = DateTime.MinValue;

                        if (lastLog != null)
                            lastLogDate = lastLog.DateEnded;

                        if (lastUsage != null)
                            lastUsageDate = lastUsage.UsageEnd;


                        usage.UsageEnd = lastLogDate == lastUsageDate ? usage.UsageEnd : lastUsageDate > lastLogDate ? lastUsageDate : lastLogDate;
                        usage.IsCurrent = false;
                        context.Entry(usage).State = EntityState.Modified;
                    }
                }

                var login = new Usage() { UserID = userID, UsageEnd = DateTime.Now, UsageTypeID = context.UsageTypes.First(u => u.UType == loginUsage).UsageTypeID, IsCurrent = true };

                context.Usages.Add(login);
                context.SaveChanges();

                return login;
            }
        }

        private Uzer InitUzer(string userName)
        {
            using (var context = new AppsEntities())
            {
                Uzer user = context.Users.FirstOrDefault(u => u.Name == userName);

                if (user == null)
                {
                    user = new Uzer() { Name = userName };
                    context.Users.Add(user);
                    context.SaveChanges();
                }

                return user;
            }
        }
    }
}
