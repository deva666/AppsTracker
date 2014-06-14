using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.DAL;
using AppsTracker.Models.EntityModels;
using System.Data.Entity;

namespace Task_Logger_Pro.Cleaner
{
    static class DBCleaner
    {
        //public static void DeleteOldLogs(int days, bool screenshotsOnly = false, bool? isLoggginStopped = null)
        //{
        //    if (App.DataLogger != null)
        //        App.DataLogger.IsLogggingStopped = true;

        //    DateTime dateTreshold = DateTime.Now.AddDays(-1d * days);
        //    if (screenshotsOnly)
        //    {
        //        using (var context = new AppsEntities())
        //        {
        //            var screenshots = (from s in context.Screenshots
        //                               where s.Date < dateTreshold
        //                               select s).ToList();
        //            DeleteScreenshots(context, screenshots);
        //            context.SaveChanges();
        //        }
        //    }
        //    else
        //    {
        //        using (var context = new AppsEntities())
        //        {
        //            var logs = (from l in context.Logs
        //                        join u in context.Usages on l.UsageID equals u.UsageID
        //                        where l.DateCreated < dateTreshold
        //                        && !u.IsCurrent
        //                        select l).Include(l => l.Screenshots).Include(l => l.Window.Application).ToList();

        //            var usages = (from u in context.Usages
        //                          where u.UsageStart < dateTreshold
        //                          && !u.IsCurrent
        //                          select u).ToList();

        //            var fileLogs = (from f in context.FileLogs
        //                            where f.Date < dateTreshold
        //                            select f).ToList();

        //            var blockedApps = (from b in context.BlockedApps
        //                               where b.Date < dateTreshold
        //                               select b).ToList();

        //            DeleteBlockedApps(context, blockedApps);

        //            DeleteFilelogs(context, fileLogs);

        //            DeleteUsages(context, usages);

        //            DeleteLogsAndScreenshots(context, logs);

        //            DeleteEmptyLogs(context);

        //            context.SaveChanges();
        //        }
        //    }
        //    if (App.DataLogger != null && isLoggginStopped.HasValue)
        //        App.DataLogger.IsLogggingStopped = isLoggginStopped.Value;
        //}

        //public static Task DeleteOldLogsAsync(int days, bool screenshotsOnly = false, bool? isLoggingStopped = null)
        //{
        //    return Task.Run(() => DeleteOldLogs(days, screenshotsOnly, isLoggingStopped));
        //}


        public static async Task DeleteOldScreenshots(int days)
        {
            using (var context = new AppsEntities())
            {
                DateTime dateTreshold = DateTime.Now.AddDays(-1d * days);
                var oldScreenshots = context.Screenshots.Where(s => s.Date < dateTreshold).ToList();
                context.Screenshots.RemoveRange(oldScreenshots);
                await context.SaveChangesAsync();
            }
        }

        private static void DeleteBlockedApps(AppsEntities context, List<BlockedApp> blockedApps)
        {
            foreach (var blockedApp in blockedApps)
            {
                if (!context.BlockedApps.Local.Any(b => b.BlockedAppID == blockedApp.BlockedAppID))
                    context.BlockedApps.Attach(blockedApp);
                context.BlockedApps.Remove(blockedApp);
            }
            context.SaveChanges();
        }

        private static void DeleteFilelogs(AppsEntities context, List<FileLog> fileLogs)
        {
            foreach (var fileLog in fileLogs)
            {
                if (!context.FileLogs.Local.Any(f => f.FileLogID == fileLog.FileLogID))
                    context.FileLogs.Attach(fileLog);
                context.FileLogs.Remove(fileLog);
            }
            context.SaveChanges();
        }

        private static void DeleteUsages(AppsEntities context, List<Usage> usages)
        {
            foreach (var usage in usages)
            {
                if (usage.IsCurrent)
                    continue;
                if (!context.Usages.Local.Any(u => u.UsageID == usage.UsageID))
                    context.Usages.Attach(usage);
                context.Usages.Remove(usage);
            }
            context.SaveChanges();
        }

        private static void DeleteLogsAndScreenshots(AppsEntities context, List<Log> logs)
        {
            foreach (var log in logs)
            {

                foreach (var screenshot in log.Screenshots.ToList())
                {
                    if (!context.Screenshots.Local.Any(s => s.ScreenshotID == screenshot.ScreenshotID))
                    {
                        context.Screenshots.Attach(screenshot);
                    }
                    context.Screenshots.Remove(screenshot);
                }

                if (!context.Logs.Local.Any(l => l.LogID == log.LogID))
                {
                    context.Logs.Attach(log);
                }
                context.Entry(log).State = System.Data.Entity.EntityState.Deleted;
            }
            context.SaveChanges();
        }

        private static void DeleteScreenshots(AppsEntities context, List<Screenshot> screenshots)
        {
            foreach (var screenshot in screenshots)
            {
                if (!context.Screenshots.Local.Any(s => s.ScreenshotID == screenshot.ScreenshotID))
                {
                    context.Screenshots.Attach(screenshot);
                }
                context.Screenshots.Remove(screenshot);
            }
            context.SaveChanges();
        }

        private static void DeleteEmptyLogs(AppsEntities context)
        {
            foreach (var window in context.Windows)
            {
                if (window.Logs.Count == 0)
                {
                    if (!context.Windows.Local.Any(w => w.WindowID == window.WindowID))
                        context.Windows.Attach(window);
                    context.Windows.Remove(window);
                }
            }

            foreach (var app in context.Applications)
            {
                if (app.Windows.Count == 0)
                {
                    if (!context.Applications.Local.Any(a => a.ApplicationID == app.ApplicationID))
                        context.Applications.Attach(app);
                    context.Applications.Remove(app);
                }
            }

            context.SaveChanges();
        }
    }
}
