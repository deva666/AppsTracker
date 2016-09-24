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
using AppsTracker.Common.Utils;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Repository
{
    [Export(typeof(IRepository))]
    public sealed class Repository : IRepository
    {
        private const int MEGA_BYTES = 1048576;
        private const decimal DB_SIZE_LIMIT = 3900m;

        public bool DBSizeOperational { get; private set; }

        public event EventHandler DbSizeCritical;

        public T GetSingle<T>(int id) where T : class, IEntity
        {
            using (var context = new AppsEntities())
            {
                return context.Set<T>().FirstOrDefault(e => e.ID == id);
            }
        }

        public async Task<T> GetSingleAsync<T>(int id) where T : class, IEntity
        {
            using (var context = new AppsEntities())
            {
                return await context.Set<T>().FirstOrDefaultAsync(e => e.ID == id);
            }
        }


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
                {
                    query = query.Include(nav);
                }
                return query.AsNoTracking().ToList();
            }
        }

        public Task<List<T>> GetAsync<T>(params Expression<Func<T, object>>[] navigations) where T : class
        {
            using (var context = new AppsEntities())
            {
                IQueryable<T> query = context.Set<T>().AsQueryable();
                foreach (var nav in navigations)
                {
                    query = query.Include(nav);
                }
                return query.AsNoTracking().ToListAsync<T>();
            }
        }

        public IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter) where T : class
        {
            using (var context = new AppsEntities())
            {
                return context.Set<T>().AsNoTracking().Where(filter).ToList();
            }
        }

        public IEnumerable<T> GetOrdered<T, TKey>(Expression<Func<T, bool>> filter,
                                                  Expression<Func<T, TKey>> selector,
                                                  int count) where T : class
        {
            using (var context = new AppsEntities())
            {
                return context.Set<T>()
                              .AsNoTracking()
                              .Where(filter)
                              .OrderBy(selector)
                              .Take(count)
                              .ToList();
            }
        }

        public IEnumerable<T> GetOrderedDesc<T, TKey>(Expression<Func<T, bool>> filter,
                                                      Expression<Func<T, TKey>> selector,
                                                      int count) where T : class
        {
            using (var context = new AppsEntities())
            {
                return context.Set<T>()
                              .AsNoTracking()
                              .Where(filter)
                              .OrderByDescending(selector)
                              .Take(count)
                              .ToList();
            }
        }

        public IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class
        {
            using (var context = new AppsEntities())
            {
                var query = context.Set<T>().AsQueryable();
                foreach (var nav in navigations)
                {
                    query = query.Include(nav);
                }
                return query.AsNoTracking().Where(filter).ToList();
            }
        }

        public Task<List<T>> GetFilteredAsync<T>(Expression<Func<T, bool>> filter) where T : class
        {
            using (var context = new AppsEntities())
            {
                var set = context.Set<T>().AsQueryable();
                return set.AsNoTracking().Where(filter).ToListAsync();
            }
        }

        public Task<List<T>> GetFilteredAsync<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class
        {
            using (var context = new AppsEntities())
            {
                IQueryable<T> query = context.Set<T>().AsQueryable();
                foreach (var nav in navigations)
                {
                    query = query.Include(nav);
                }
                return query.AsNoTracking().Where(filter).ToListAsync();
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

        public async Task SaveNewEntityAsync<T>(T item) where T : class
        {
            using (var context = new AppsEntities())
            {
                context.Set<T>().Add(item);
                await context.SaveChangesAsync();
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

        public async Task DeleteScreenshotsById(IEnumerable<Int32> ids)
        {
            using (var context = new AppsEntities())
            {
                var screenshots = await context.Set<Screenshot>().Where(s => ids.Contains(s.ID)).ToListAsync();
                context.Screenshots.RemoveRange(screenshots);
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

        public void DeleteByIds<T>(IEnumerable<Int32> ids) where T : class, IEntity
        {
            using (var context = new AppsEntities())
            {
                var items = context.Set<T>().Where(e => ids.Contains(e.ID));
                context.Set<T>().RemoveRange(items);
                context.SaveChanges();
            }
        }

        public async Task DeleteByIdsAsync<T>(IEnumerable<Int32> ids) where T : class, IEntity
        {
            using (var context = new AppsEntities())
            {
                var items = context.Set<T>().Where(e => ids.Contains(e.ID));
                context.Set<T>().RemoveRange(items);
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


        public async Task<int> DeleteOldScreenshotsAsync(int daysBackwards)
        {
            using (var context = new AppsEntities())
            {
                DateTime dateTreshold = DateTime.Now.AddDays(-1d * daysBackwards);
                var oldScreenshots = context.Screenshots.Where(s => s.Date < dateTreshold).ToList();
                context.Screenshots.RemoveRange(oldScreenshots);
                return await context.SaveChangesAsync();
            }
        }

        public async Task DeleteOldLogsAsync(int daysTreshold)
        {
            var now = DateTime.Now.Date;
            var oldDate = now.AddDays(-1 * daysTreshold);
            using (var context = new AppsEntities())
            {
                var logsTask = context.Logs.Where(l => l.DateCreated < oldDate).ToListAsync();
                var screenshotsTask = context.Screenshots.Where(s => s.Date < oldDate).ToListAsync();
                var usagesTask = context.Usages.Where(u => u.UsageStart < oldDate && u.IsCurrent == false)
                    .Include(u => u.Logs).ToListAsync();

                await Task.WhenAll(logsTask, screenshotsTask, usagesTask);

                var usages = usagesTask.Result;
                var usageLogs = usages.SelectMany(u => u.Logs);
                var logs = logsTask.Result;
                logs.AddRange(usageLogs);
                logs = logs.Distinct().ToList();

                var screenshotsLogs = logs.SelectMany(l => l.Screenshots);
                var screenshots = screenshotsTask.Result;
                screenshots.AddRange(screenshotsLogs);
                screenshots = screenshots.Distinct().ToList();

                context.Screenshots.RemoveRange(screenshots);
                context.Logs.RemoveRange(logs);
                context.Usages.RemoveRange(usages);

                await context.SaveChangesAsync();
            }

            await DeleteEmptyLogsAsync();
        }

        private async Task DeleteEmptyLogsAsync()
        {
            using (var context = new AppsEntities())
            {
                var emptyWindows = await context.Windows.Where(w => w.Logs.Count == 0).ToListAsync();
                foreach (var window in emptyWindows)
                {
                    if (!context.Windows.Local.Any(w => w.ID == window.ID))
                        context.Windows.Attach(window);
                    context.Windows.Remove(window);
                    await context.SaveChangesAsync();
                }

                var appsInCategories = await context.AppCategories.SelectMany(c => c.Applications).ToListAsync();
                var appsWithLimits = await context.Applications.Where(a => a.Limits.Count > 0).ToListAsync();

                foreach (var app in context.Applications.Where(a => a.Windows.Count == 0))
                {
                    if (appsInCategories.Contains(app) || appsWithLimits.Contains(app))
                        continue;

                    if (!context.Applications.Local.Any(a => a.ID == app.ID))
                        context.Applications.Attach(app);
                    context.Applications.Remove(app);
                    await context.SaveChangesAsync();
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

        public void CheckUnfinishedEntries()
        {
            using (var context = new AppsEntities())
            {
                var unfinishedUsages = context.Usages
                                              .Where(u => u.IsCurrent && u.UsageType == UsageTypes.Login)
                                              .AsNoTracking()
                                              .ToList();
                var unfinishedLogs = context.Logs
                                            .Where(l => !l.Finished)
                                            .AsNoTracking()
                                            .ToList();

                if (unfinishedUsages.Count > 0)
                {
                    foreach (var usage in unfinishedUsages)
                    {
                        SaveUnfinishedUsage(usage);
                    }
                }

                if (unfinishedLogs.Count > 0)
                {
                    unfinishedLogs.ForEach(l =>
                    {
                        l.Finished = true;
                        context.Entry(l).State = EntityState.Modified;
                    });
                    context.SaveChanges();
                }
            }
        }

        private void SaveUnfinishedUsage(Usage usage)
        {
            using (var context = new AppsEntities())
            {
                var lastLog = context.Logs.Where(l => l.UsageID == usage.UsageID)
                                          .OrderByDescending(l => l.DateCreated)
                                          .FirstOrDefault();
                var lastUsage = context.Usages.Where(u => u.SelfUsageID == usage.UsageID)
                                              .OrderByDescending(u => u.UsageEnd)
                                              .FirstOrDefault();

                var lastLogDate = DateTime.MinValue;
                var lastUsageDate = DateTime.MinValue;

                if (lastLog != null)
                    lastLogDate = lastLog.DateEnded;

                if (lastUsage != null)
                    lastUsageDate = lastUsage.UsageEnd;

                var latestDateKnown = new DateTime[] { lastLogDate, lastUsageDate }
                                            .OrderByDescending(d => d.Ticks)
                                            .FirstOrDefault();

                if (latestDateKnown == DateTime.MinValue)
                    latestDateKnown = usage.UsageStart;

                usage.UsageEnd = latestDateKnown;
                usage.IsCurrent = false;

                context.Entry(usage).State = EntityState.Modified;
                context.SaveChanges();
            }
        }
    }
}
