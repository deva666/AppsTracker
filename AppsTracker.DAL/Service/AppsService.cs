#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Service
{
    public sealed class AppsService : IAppsService
    {
        public IEnumerable<T> Get<T>() where T : class
        {
            using (var context = new AppsEntities())
            {
                return context.Set<T>().AsNoTracking().ToList();
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

        public int DeleteScreenshots(IEnumerable<Log> logs)
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
                context.SaveChanges();
            }
            return count;
        }

        public Aplication GetApp(int userID, string appName)
        {
            using (var context = new AppsEntities())
            {
                return context.Applications.FirstOrDefault(a => a.UserID == userID
                    && a.Name == appName);
            }
        }
    }
}
