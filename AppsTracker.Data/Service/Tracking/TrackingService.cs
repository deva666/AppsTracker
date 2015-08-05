#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppsTracker.Common.Communication;
using AppsTracker.Common.Utils;
using AppsTracker.Communication;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;
using AppsTracker.Data.Utils;


namespace AppsTracker.Data.Service
{
    [Export(typeof(ITrackingService))]
    public sealed class TrackingService : ITrackingService
    {
        private readonly IMediator mediator;
        private readonly ISyncContext syncContext;

        private volatile bool isDateRangeFiltered;

        private int userID;

        public int UserID
        {
            get { return userID; }
            private set { Interlocked.Exchange(ref userID, value); }
        }


        private int usageID;

        public int UsageID
        {
            get { return usageID; }
            private set { Interlocked.Exchange(ref usageID, value); }
        }

        private int selectedUserID;

        public int SelectedUserID
        {
            get { return selectedUserID; }
            private set { Interlocked.Exchange(ref selectedUserID, value); }
        }


        private long dateFromTicks;

        public DateTime DateFrom
        {
            get { return new DateTime(Interlocked.Read(ref dateFromTicks)); }
            set { Interlocked.Exchange(ref dateFromTicks, value.Ticks); }
        }


        private long dateToTicks;

        public DateTime DateTo
        {
            get
            {
                if (isDateRangeFiltered)
                    return new DateTime(Interlocked.Read(ref dateToTicks));
                else
                    return DateTime.Now;
            }
            set
            {
                isDateRangeFiltered = true;
                Interlocked.Exchange(ref dateToTicks, value.Ticks);
            }
        }

        public string SelectedUserName { get; private set; }

        public Uzer SelectedUser { get; private set; }

        public string UserName { get; private set; }


        [ImportingConstructor]
        public TrackingService(IMediator mediator, ISyncContext syncContext)
        {
            this.mediator = mediator;
            this.syncContext = syncContext;
        }

        public void Initialize(Uzer uzer, int usageID)
        {
            Ensure.NotNull(uzer, "uzer");

            UserID = uzer.UserID;
            UserName = uzer.Name;
            SelectedUserID = UserID;
            SelectedUserName = UserName;
            DateFrom = GetFirstDate(SelectedUserID);
            UsageID = usageID;
        }


        public void ChangeUser(Uzer uzer)
        {
            Ensure.NotNull(uzer, "uzer");

            SelectedUserID = uzer.UserID;
            SelectedUserName = uzer.Name;
            SelectedUser = uzer;
            ClearDateFilter();
        }

        public void ClearDateFilter()
        {
            DateFrom = GetFirstDate(SelectedUserID);
            isDateRangeFiltered = false;
        }

        public Log CreateNewLog(string windowTitle, int usageID, int userID, AppInfo appInfo, out bool newApp)
        {
            Ensure.NotNull(appInfo, "appInfo");

            using (var context = new AppsEntities())
            {
                newApp = false;
                string appName = (!string.IsNullOrEmpty(appInfo.Name) ? appInfo.Name : !string.IsNullOrEmpty(appInfo.FullName) ? appInfo.FullName : appInfo.FileName);
                var app = context.Applications.FirstOrDefault(a => a.UserID == userID
                                                        && a.Name == appName);

                if (app == null)
                {
                    app = new Aplication(appInfo) { UserID = userID };
                    context.Applications.Add(app);

                    newApp = true;
                }

                var window = context.Windows.FirstOrDefault(w => w.Title == windowTitle
                                                     && w.Application.ApplicationID == app.ApplicationID);

                if (window == null)
                {
                    window = new Window(windowTitle) { Application = app };
                    context.Windows.Add(window);
                }

                var log = new Log(window, usageID, new Guid());
                context.Logs.Add(log);
                context.SaveChanges();

                return log;
            }
        }


        public Task<Log> CreateLogEntryAsync(LogInfo logInfo)
        {
            return Task.Run(() => CreateLogEntry(logInfo));
        }

        public Log CreateLogEntry(LogInfo logInfo)
        {
            using (var context = new AppsEntities())
            {
                var appInfo = logInfo.AppInfo;
                var newApp = false;
                string appName = (!string.IsNullOrEmpty(appInfo.Name) ? appInfo.Name : !string.IsNullOrEmpty(appInfo.FullName) ? appInfo.FullName : appInfo.FileName);
                var app = context.Applications.FirstOrDefault(a => a.UserID == userID
                                                        && a.Name == appName);

                if (app == null)
                {
                    app = new Aplication(appInfo) { UserID = userID };
                    context.Entry(app).State = EntityState.Added;

                    newApp = true;
                }
                else
                {
                    context.Entry(app).State = EntityState.Unchanged;
                }

                var window = context.Windows.FirstOrDefault(w => w.Title == logInfo.WindowTitle
                                            && w.Application.ApplicationID == app.ApplicationID);

                if (window == null)
                {
                    window = new Window(logInfo.WindowTitle) { Application = app };
                    context.Entry(window).State = EntityState.Added;
                }
                else
                {
                    context.Entry(window).State = EntityState.Unchanged;
                }

                var log = new Log(window, usageID, logInfo.Guid)
                {
                    DateCreated = logInfo.Start,
                    UtcDateCreated = logInfo.UtcStart,
                    DateEnded = logInfo.End,
                    UtcDateEnded = logInfo.UtcEnd,
                };

                context.Logs.Add(log);
                context.SaveChanges();
                context.Entry(log).State = EntityState.Detached;

                if (newApp)
                    syncContext.Invoke(() => mediator.NotifyColleagues(MediatorMessages.APPLICATION_ADDED, app));

                return log;
            }
        }

        public async Task EndLogEntry(LogInfo logInfo)
        {
            using (var context = new AppsEntities())
            {
                var log = logInfo.Log;
                context.Logs.Attach(log);
                if (log.Window != null)
                    context.Windows.Attach(log.Window);
                if (log.Window != null && log.Window.Application != null)
                    context.Applications.Attach(log.Window.Application);
                log.Finish();

                if (logInfo.HasScreenshots)
                {
                    foreach (var screenshot in logInfo.Screenshots)
                    {
                        screenshot.LogID = log.LogID;
                    }
                    context.Screenshots.AddRange(logInfo.Screenshots);
                }

                context.Entry(log).State = EntityState.Modified;

                await context.SaveChangesAsync();
            }
        }

        public async Task EndLogEntry(Log log)
        {
            using (var context = new AppsEntities())
            {
                context.Logs.Attach(log);
                if (log.Window != null)
                    context.Windows.Attach(log.Window);
                if (log.Window != null && log.Window.Application != null)
                    context.Applications.Attach(log.Window.Application);
                log.Finish();
                context.Entry(log).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }

        public Task<Aplication> GetAppAsync(AppInfo appInfo, Int32 userId = default(Int32))
        {
            return Task.Run(() => GetApp(appInfo, userId));
        }

        public Aplication GetApp(AppInfo appInfo, Int32 userId = default(Int32))
        {
            if (appInfo == null)
                return null;

            using (var context = new AppsEntities())
            {
                var name = !string.IsNullOrEmpty(appInfo.Name)
                    ? appInfo.Name.Truncate(250) : !string.IsNullOrEmpty(appInfo.FullName)
                    ? appInfo.FullName.Truncate(250) : appInfo.FileName.Truncate(250);
                var requestedUserId = userId == default(int) ? this.userID : userID;
                var app = context.Applications.FirstOrDefault(a => a.Name == name && a.UserID == requestedUserId);
                if (app != null)
                    app.AppInfo = appInfo;
                return app;
            }
        }

        public Usage LoginUser(int userID)
        {
            using (var context = new AppsEntities())
            {
                if (context.Usages.Where(u => u.IsCurrent && u.UsageType == UsageTypes.Login).Count() > 0)
                {
                    var failedSaveUsage = context.Usages.Where(u => u.IsCurrent && u.UsageType == UsageTypes.Login).ToList();
                    foreach (var usage in failedSaveUsage)
                    {
                        var lastLog = context.Logs.Where(l => l.UsageID == usage.UsageID).OrderByDescending(l => l.DateCreated).FirstOrDefault();
                        var unfinishedLogs = context.Logs.Where(l => l.Finished == false);
                        var lastUsage = context.Usages.Where(u => u.SelfUsageID == usage.UsageID).OrderByDescending(u => u.UsageEnd).FirstOrDefault();
                        var lastUnfinishedLog = unfinishedLogs.OrderByDescending(l => l.DateCreated).FirstOrDefault();

                        unfinishedLogs.ForEachAction(l => l.Finished = true);
                        unfinishedLogs.AttachToContextAsModified(context);

                        DateTime lastLogDate = DateTime.MinValue;
                        DateTime lastUsageDate = DateTime.MinValue;
                        DateTime lastUnfinishedLogDate = DateTime.MinValue;

                        if (lastLog != null)
                            lastLogDate = lastLog.DateEnded;

                        if (lastUsage != null)
                            lastUsageDate = lastUsage.UsageEnd;

                        if (lastUnfinishedLog != null)
                            lastUnfinishedLogDate = lastUnfinishedLog.DateEnded;

                        var latestDateKnown = new DateTime[] { lastLogDate, lastUsageDate, lastUnfinishedLogDate }.OrderByDescending(d => d.Ticks).FirstOrDefault();

                        if (latestDateKnown == DateTime.MinValue)
                            latestDateKnown = usage.UsageStart;

                        usage.UsageEnd = latestDateKnown;
                        usage.IsCurrent = false;
                        context.Entry(usage).State = EntityState.Modified;
                    }
                }

                var login = new Usage() { UserID = userID, UsageEnd = DateTime.Now, UsageType = UsageTypes.Login, IsCurrent = true };

                context.Usages.Add(login);
                context.SaveChanges();

                return login;
            }
        }

        public Uzer GetUzer(string userName)
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

        public DateTime GetFirstDate(int userID)
        {
            using (var context = new AppsEntities())
            {
                return context.Usages.Count(u => u.UserID == userID) == 0 ? DateTime.Now.Date
                    : context.Usages.Where(u => u.UserID == userID).Select(u => u.UsageStart).Min();
            }
        }


        public long GetDayDuration(Aplication app)
        {
            Ensure.NotNull(app, "app");

            using (var context = new AppsEntities())
            {
                var loadedApp = context.Applications
                                       .Include(a => a.Windows.Select(w => w.Logs))
                                       .First(a => a.ApplicationID == app.ApplicationID);

                return loadedApp.Windows.SelectMany(w => w.Logs)
                                        .Where(l => l.DateCreated >= DateTime.Now.Date)
                                        .Sum(l => l.Duration);
            }
        }


        public long GetWeekDuration(Aplication app)
        {
            Ensure.NotNull(app, "app");

            DateTime now = DateTime.Today;
            int delta = DayOfWeek.Monday - now.DayOfWeek;
            if (delta > 0)
                delta -= 7;
            DateTime weekBegin = now.AddDays(delta);
            DateTime weekEnd = weekBegin.AddDays(6);

            using (var context = new AppsEntities())
            {
                var loadedApp = context.Applications
                                       .Include(a => a.Windows.Select(w => w.Logs))
                                       .First(a => a.ApplicationID == app.ApplicationID);
                return loadedApp.Windows
                                .SelectMany(w => w.Logs)
                                .Where(l => l.DateCreated >= weekBegin && l.DateCreated <= weekEnd)
                                .Sum(l => l.Duration);
            }
        }
    }
}
