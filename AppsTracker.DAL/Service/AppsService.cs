using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

using AppsTracker.Common.Utils;
using AppsTracker.Models.EntityModels;
using AppsTracker.Models.Proxy;

namespace AppsTracker.DAL.Service
{
    public sealed class AppsService : IAppsService
    {
        public IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter) where T : class
        {
            using (var context = new AppsEntities())
            {
                return context.Set<T>().AsNoTracking().Where(filter).ToList();
            }
        }

        public IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class
        {
            using (var context = new AppsEntities())
            {
                var query = context.Set<T>().AsQueryable();
                foreach (var nav in navigations)
                    query = query.Include(nav);
                return query.AsNoTracking().Where(filter).ToList();
            }
        }

        public IQueryable<T> GetQueryable<T>() where T : class
        {
            using (var context = new AppsEntities())
            {
                return context.Set<T>().AsNoTracking();
            }
        }

        public Log CreateNewLog(string windowTitle, int usageID, int userID, IAppInfo appInfo, out bool newApp)
        {
            using (var context = new AppsEntities())
            {
                newApp = false;
                string appName = (!string.IsNullOrEmpty(appInfo.ProcessName) ? appInfo.ProcessName : !string.IsNullOrEmpty(appInfo.ProcessRealName) ? appInfo.ProcessRealName : appInfo.ProcessFileName);
                Aplication app = context.Applications.FirstOrDefault(a => a.UserID == userID
                                                        && a.Name == appName);

                if (app == null)
                {
                    app = new Aplication(appInfo.ProcessName,
                                                    appInfo.ProcessFileName,
                                                    appInfo.ProcessVersion,
                                                    appInfo.ProcessDescription,
                                                    appInfo.ProcessCompany,
                                                    appInfo.ProcessRealName) { UserID = userID };
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

                context.SaveChanges();

                return new Log(window.WindowID, usageID);
            }
        }

        public T GetSingle<T>(Expression<Func<T, bool>> filter) where T : class
        {
            using (var context = new AppsEntities())
            {
                return context.Set<T>().AsNoTracking().Where(filter).FirstOrDefault();
            }
        }

        public T GetSingle<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class
        {
            using (var context = new AppsEntities())
            {
                var query = context.Set<T>();
                foreach (var nav in navigations)
                    query.Include(nav);
                return query.AsNoTracking().Where(filter).FirstOrDefault();
            }
        }

        public void Add<T>(T item) where T : class
        {
            using (var context = new AppsEntities())
            {
                context.Set<T>().Add(item);
                context.SaveChanges();
            }
        }

        public Uzer InitUzer(string userName)
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

        public Usage InitLogin(int userID)
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

        public DateTime GetStartDate(int userID)
        {
            using (var context = new AppsEntities())
            {
                return context.Usages.Count(u => u.UserID == userID) == 0 ? DateTime.Now.Date
                    : context.Usages.Where(u => u.UserID == userID).Select(u => u.UsageStart).Min();
            }
        }

        public IList<AppsToBlock> AddToBlockedList(List<Aplication> apps, string blockUsername, int loadUserID)
        {
            Ensure.NotNull(apps, "apps");
            Ensure.NotNull(blockUsername, "blockUsername");

            using (var context = new AppsEntities())
            {
                if (blockUsername.ToUpper() == "ALL USERS")
                {
                    foreach (var user in context.Users)
                    {
                        foreach (var app in apps)
                        {
                            if (app.Description.ToUpper() != "APPS TRACKER" || !string.IsNullOrEmpty(app.WinName))
                            {
                                if (!context.AppsToBlocks.Any(a => a.ApplicationID == app.ApplicationID
                                                            && a.UserID == user.UserID))
                                {
                                    AppsToBlock appToBlock = new AppsToBlock(user, app);
                                    context.AppsToBlocks.Add(appToBlock);
                                }
                            }
                        }
                    }
                }
                else
                {
                    var uzer = context.Users.FirstOrDefault(u => u.Name == blockUsername);
                    foreach (var app in apps)
                    {
                        if (app.Description.ToUpper() != "APPS TRACKER" || !string.IsNullOrEmpty(app.WinName))
                        {
                            if (!context.AppsToBlocks.Any(a => a.ApplicationID == app.ApplicationID
                                                            && a.UserID == uzer.UserID))
                            {
                                AppsToBlock appToBlock = new AppsToBlock(uzer, app);
                                context.AppsToBlocks.Add(appToBlock);
                            }
                        }
                    }
                }

                context.SaveChanges();

                var notifyList = context.AppsToBlocks.Where(a => a.UserID == loadUserID)
                                                .Include(a => a.Application)
                                                .ToList();
                return notifyList;
            }

        }
    }
}
