#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.Data.Db;
using AppsTracker.Data.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.ComponentModel.Composition;
using System.Threading.Tasks;

namespace AppsTracker.Service
{
    [Export(typeof(IDataService))]
    public sealed class DataService : IDataService
    {
        public IEnumerable<T> Get<T>() where T : class
        {
            using (var context = new AppsEntities())
            {
                return context.Set<T>().AsNoTracking().ToList();
            }
        }

        public IEnumerable<T> Get<T>(params Expression<Func<T, object>>[] navigations) where T : class
        {
            using (var context = new AppsEntities())
            {
                var query = context.Set<T>().AsQueryable();
                foreach (var nav in navigations)
                    query = query.Include(nav);
                return query.AsNoTracking().ToList();
            }
        }

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

        public async Task DeleteScreenshotsInLogs(IEnumerable<Log> logs)
        {
            var count = logs.Select(l => l.Screenshots).Count();
            using (var context = new AppsEntities())
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
                }
                await context.SaveChangesAsync();
            }
        }


        public async Task DeleteScreenshots(IEnumerable<Screenshot> screenshots)
        {
            using (var context = new AppsEntities())
            {
                foreach (var shot in screenshots)
                {
                    context.Screenshots.Attach(shot);
                }
                context.Screenshots.RemoveRange(screenshots);
                await context.SaveChangesAsync();
            }
        }


        public int DeleteOldScreenshots(int daysBackwards)
        {
            using (var context = new AppsEntities())
            {
                DateTime dateTreshold = DateTime.Now.AddDays(-1d * daysBackwards);
                var oldScreenshots = context.Screenshots.Where(s => s.Date < dateTreshold).ToList();
                context.Screenshots.RemoveRange(oldScreenshots);
                return context.SaveChanges();
            }
        }

        public void DeleteOldLogs(int daysTreshold)
        {
            var now = DateTime.Now.Date;
            var oldDate = now.AddDays(-1 * daysTreshold);
            using (var context = new AppsEntities())
            {
                var logs = context.Logs.Where(l => l.DateCreated < oldDate).ToList();
                var screenshots = context.Screenshots.Where(s => s.Date < oldDate).ToList();
                var usages = context.Usages.Where(u => u.UsageStart < oldDate && u.IsCurrent == false).ToList();
                var blockedApps = context.BlockedApps.Where(b => b.Date < oldDate).ToList();

                var usageLogs = usages.SelectMany(u => u.Logs);
                logs.AddRange(usageLogs);
                logs = logs.Distinct().ToList();

                var screenshotsLogs = logs.SelectMany(l => l.Screenshots);
                screenshots.AddRange(screenshotsLogs);
                screenshots = screenshots.Distinct().ToList();

                context.Screenshots.RemoveRange(screenshots);
                context.Logs.RemoveRange(logs);
                context.Usages.RemoveRange(usages);
                context.BlockedApps.RemoveRange(blockedApps);

                context.SaveChanges();
            }

            DeleteEmptyLogs();
        }

        private void DeleteEmptyLogs()
        {
            using (var context = new AppsEntities())
            {
                foreach (var window in context.Windows.Where(w => w.Logs.Count == 0))
                {
                    if (!context.Windows.Local.Any(w => w.WindowID == window.WindowID))
                        context.Windows.Attach(window);
                    context.Windows.Remove(window);
                    context.SaveChanges();
                }

                foreach (var app in context.Applications.Where(a => a.Windows.Count == 0))
                {
                    if (!context.Applications.Local.Any(a => a.ApplicationID == app.ApplicationID))
                        context.Applications.Attach(app);
                    context.Applications.Remove(app);
                    context.SaveChanges();
                }
            }
        }
    }
}
