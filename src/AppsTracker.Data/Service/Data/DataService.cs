#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;
using AppsTracker.Common.Utils;

namespace AppsTracker.Data.Service
{
    [Export(typeof(IDataService))]
    public sealed class DataService : IDataService
    {
        private const int MEGA_BYTES = 1048576;
        private const decimal DB_SIZE_LIMIT = 3900m;

        public bool DBSizeOperational { get; private set; }

        public event EventHandler DbSizeCritical;


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

        public void SaveModifiedEntity<T>(T item) where T : class
        {
            using (var context = new AppsEntities())
            {
                context.Entry<T>(item).State = EntityState.Modified;
                context.SaveChanges();
            }
        }

        public async Task SaveModifiedEntityAsync<T>(T item) where T : class
        {
            using (var context = new AppsEntities())
            {
                context.Entry<T>(item).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
        }


        public void SaveNewEntity<T>(T item) where T : class
        {
            using (var context = new AppsEntities())
            {
                context.Set<T>().Add(item);
                context.SaveChanges();
            }
        }

        public void SaveModifiedEntityRange<T>(IEnumerable<T> items) where T : class
        {
            using (var context = new AppsEntities())
            {
                foreach (var item in items)
                {
                    context.Entry(item).State = EntityState.Modified;
                }
                context.SaveChanges();
            }
        }

        public async Task SaveModifiedEntityRangeAsync<T>(IEnumerable<T> items) where T : class
        {
            using (var context = new AppsEntities())
            {
                foreach (var item in items)
                {
                    context.Entry(item).State = EntityState.Modified;
                }
                await context.SaveChangesAsync();
            }
        }

        public void SaveNewEntityRange<T>(IEnumerable<T> items) where T : class
        {
            using (var context = new AppsEntities())
            {
                context.Set<T>().AddRange(items);
                context.SaveChanges();
            }
        }

        public async Task SaveNewEntityRangeAsync<T>(IEnumerable<T> items) where T : class
        {
            using (var context = new AppsEntities())
            {
                context.Set<T>().AddRange(items);
                await context.SaveChangesAsync();
            }
        }

        public void DeleteEntityRange<T>(IEnumerable<T> range) where T : class
        {
            using (var context = new AppsEntities())
            {
                foreach (var item in range)
                {
                    context.Entry<T>(item).State = EntityState.Deleted;
                }
                context.SaveChanges();
            }
        }

        public async Task DeleteEntityRangeAsync<T>(IEnumerable<T> range) where T : class
        {
            using (var context = new AppsEntities())
            {
                foreach (var item in range)
                {
                    context.Entry<T>(item).State = EntityState.Deleted;
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

                var usageLogs = usages.SelectMany(u => u.Logs);
                logs.AddRange(usageLogs);
                logs = logs.Distinct().ToList();

                var screenshotsLogs = logs.SelectMany(l => l.Screenshots);
                screenshots.AddRange(screenshotsLogs);
                screenshots = screenshots.Distinct().ToList();

                context.Screenshots.RemoveRange(screenshots);
                context.Logs.RemoveRange(logs);
                context.Usages.RemoveRange(usages);

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

                var appsInCategories = context.AppCategories.SelectMany(c => c.Applications).ToList();
                var appsWithLimits = context.Applications.Where(a => a.Limits.Count > 0).ToList();

                foreach (var app in context.Applications.Where(a => a.Windows.Count == 0))
                {
                    if (appsInCategories.Contains(app) || appsWithLimits.Contains(app))
                        continue;

                    if (!context.Applications.Local.Any(a => a.ApplicationID == app.ApplicationID))
                        context.Applications.Attach(app);
                    context.Applications.Remove(app);
                    context.SaveChanges();
                }
            }
        }

        public Task<decimal> GetDBSizeAsync()
        {
            return Task<decimal>.Run(new Func<decimal>(GetDBSize));
        }

        public decimal GetDBSize()
        {
            try
            {
                FileInfo file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "AppService", "appsdb.sdf"));
                decimal size = Math.Round((decimal)file.Length / MEGA_BYTES, 2);
                if (size >= DB_SIZE_LIMIT)
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
    }
}
