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
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;
using AppsTracker.Data.Utils;

namespace AppsTracker.Service
{
    [Export(typeof(ITrackingService))]
    public sealed class TrackingService : ITrackingService
    {
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


        public void Initialize(Uzer uzer, int usageID)
        {
            UserID = uzer.UserID;
            UserName = uzer.Name;
            SelectedUserID = UserID;
            SelectedUserName = UserName;
            DateFrom = GetFirstDate(SelectedUserID);
            UsageID = usageID;
        }


        public void ChangeUser(Uzer uzer)
        {
            if (uzer == null)
                throw new ArgumentNullException("User");
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

        public Log CreateNewLog(string windowTitle, int usageID, int userID, IAppInfo appInfo, out bool newApp)
        {
            using (var context = new AppsEntities())
            {
                newApp = false;
                string appName = (!string.IsNullOrEmpty(appInfo.Name) ? appInfo.Name : !string.IsNullOrEmpty(appInfo.FullName) ? appInfo.FullName : appInfo.FileName);
                Aplication app = context.Applications.FirstOrDefault(a => a.UserID == userID
                                                        && a.Name == appName);

                if (app == null)
                {
                    app = new Aplication(appInfo) { UserID = userID };
                    context.Applications.Add(app);

                    newApp = true;
                }

                Window window = context.Windows.FirstOrDefault(w => w.Title == windowTitle
                                                     && w.Application.ApplicationID == app.ApplicationID);

                if (window == null)
                {
                    window = new Window(windowTitle) { Application = app };
                    context.Windows.Add(window);
                }

                var log = new Log(window, usageID);
                context.Logs.Add(log);
                context.SaveChanges();

                return log;
            }
        }


        public Aplication GetApp(IAppInfo appInfo)
        {
            if (appInfo == null)
                return null;

            using (var context = new AppsEntities())
            {
                var name = !string.IsNullOrEmpty(appInfo.Name) ? appInfo.Name.Truncate(250) : !string.IsNullOrEmpty(appInfo.FullName) ? appInfo.FullName.Truncate(250) : appInfo.FileName.Truncate(250);
                var app = context.Applications.First(a => a.Name == name);
                return app;
            }
        }

        public Usage LoginUser(int userID)
        {
            using (var context = new AppsEntities())
            {
                string loginUsage = UsageTypes.Login.ToString();

                if (context.Usages.Where(u => u.IsCurrent && u.UsageType.ToString() == loginUsage).Count() > 0)
                {
                    var failedSaveUsage = context.Usages.Where(u => u.IsCurrent && u.UsageType.ToString() == loginUsage).ToList();
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
    }
}
