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
using AppsTracker.Common.Utils;
using AppsTracker.Models.EntityModels;

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
    }
}
