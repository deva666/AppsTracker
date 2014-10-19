using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.DAL;
using AppsTracker.Models.EntityModels;
using System.Data.Entity;
using AppsTracker.DAL.Repos;
using System.Diagnostics;

namespace AppsTracker.Cleaner
{
    static class DBCleaner
    {

        public static void DeleteOldScreenshots(int days)
        {
            using (var context = new AppsEntities())
            {
                DeleteScreenshotsFromDB(days, context);
                context.SaveChanges();
            }
        }

        public static void CleanLogs(int days)
        {
            Debug.Assert(days > 0, "Days must be a positive number");

            var now = DateTime.Now.Date;
            var oldDate = now.AddDays(-1 * days);
            using (var context = new AppsEntities())
            {
                var logs = context.Logs.Where(l => l.DateCreated <= oldDate).ToList();
                var screenshots = context.Screenshots.Where(s => s.Date <= oldDate).ToList();
                var usages = context.Usages.Where(u => u.UsageStart <= oldDate).ToList();
                var blockedApps = context.BlockedApps.Where(b => b.Date <= oldDate).ToList();

                context.Screenshots.RemoveRange(screenshots);
                context.Logs.RemoveRange(logs);
                context.Usages.RemoveRange(usages);
                context.BlockedApps.RemoveRange(blockedApps);

                DeleteEmptyLogs(context);

                context.SaveChanges();
            }
        }

        public static Task CleanLogsAsync(int days)
        {
            return Task.Run(() => CleanLogs(days));
        }

        public static async Task DeleteOldScreenshotsAsync(int days, bool continueOnCapturedContext = true)
        {
            using (var context = new AppsEntities())
            {
                DeleteScreenshotsFromDB(days, context);
                await context.SaveChangesAsync().ConfigureAwait(continueOnCapturedContext);
            }
        }

        private static void DeleteScreenshotsFromDB(int days, AppsEntities context)
        {
            DateTime dateTreshold = DateTime.Now.AddDays(-1d * days);
            var oldScreenshots = context.Screenshots.Where(s => s.Date < dateTreshold).ToList();
            context.Screenshots.RemoveRange(oldScreenshots);
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

        private static void DeleteEmptyLogs(AppsEntities context, bool saveChanges = false)
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

            if (saveChanges)
                context.SaveChanges();
        }
    }
}
