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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;
using AppsTracker.Data.Utils;

namespace AppsTracker.Service
{
    [Export(typeof(ILoggingService))]
    public sealed class LoggingService : ILoggingService
    {
        private bool isDateRangeFiltered;
        private DateTime dateFrom;
        private DateTime dateTo;

        public bool DBSizeOperational { get; private set; }
        public int UserID { get; private set; }
        public string UserName { get; private set; }
        public int UsageID { get; private set; }
        public int SelectedUserID { get; private set; }
        public string SelectedUserName { get; private set; }
        public Uzer SelectedUser { get; private set; }

        public DateTime DateFrom
        {
            get { return dateFrom; }
            set { dateFrom = value; }
        }

        public DateTime DateTo
        {
            get
            {
                if (isDateRangeFiltered)
                    return dateTo;
                else
                    return DateTime.Now;
            }
            set
            {
                isDateRangeFiltered = true;
                dateTo = value;
            }
        }

        public event EventHandler DbSizeCritical;

        public void Initialize(Uzer uzer, int usageID)
        {
            UserID = uzer.UserID;
            UserName = uzer.Name;
            SelectedUserID = UserID;
            SelectedUserName = UserName;
            dateFrom = GetFirstDate(SelectedUserID);
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
            dateFrom = GetFirstDate(SelectedUserID);
            isDateRangeFiltered = false;
        }

        public decimal GetDBSize()
        {
            try
            {
                FileInfo file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "appsdb.sdf"));
                decimal size = Math.Round((decimal)file.Length / 1048576, 2);
                if (size >= 3900m)
                {
                    DBSizeOperational = false;
                    DbSizeCritical.InvokeSafely(this, EventArgs.Empty);
                }
                else
                    DBSizeOperational = true;
                return size;
            }
            catch 
            {
                return -1;
            }
        }

        public Task<decimal> GetDBSizeAsync()
        {
            return Task<decimal>.Run(new Func<decimal>(GetDBSize));
        }

        public async Task SaveModifiedLogAsync(Log log)
        {
            using (var context = new AppsEntities())
            {
                context.Entry<Log>(log).State = System.Data.Entity.EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }

        public async Task SaveNewScreenshotAsync(Screenshot screenshot)
        {
            using (var context = new AppsEntities())
            {
                context.Screenshots.Add(screenshot);
                await context.SaveChangesAsync();
            }
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

        public async Task SaveNewUsageAsync(UsageTypes usagetype, Usage usage)
        {
            using (var context = new AppsEntities())
            {
                usage.UsageType = usagetype;
                context.Usages.Add(usage);
                await context.SaveChangesAsync();
            }
        }

        public async Task SaveModifiedUsageAsync(Usage usage)
        {
            if (usage == null)
                return;

            usage.UsageEnd = DateTime.Now;
            using (var context = new AppsEntities())
            {
                context.Entry<Usage>(usage).State = EntityState.Modified;
                await context.SaveChangesAsync();
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
