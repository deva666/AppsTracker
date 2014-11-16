#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

using AppsTracker.Common.Utils;
using AppsTracker.DAL;

namespace AppsTracker.Logging
{
    internal sealed class LogCleanerHelper : IDisposable
    {
        private int _days;

        public int Days
        {
            get
            {
                return _days;
            }
            set
            {
                Ensure.Condition<InvalidOperationException>(value >= 15, "Minimum value must be 15");
                _days = value;
            }
        }

        public LogCleanerHelper(int daysToDelete)
        {
            Days = daysToDelete;
            CleanAsync();
        }

        private bool CheckLogs()
        {
            using (var context = new AppsEntities())
            {
                DateTime date = DateTime.Now.Date.AddDays(-1d * _days);
                return context.Logs.Where(l => l.DateCreated < date)
                                    .Count() > 0;
            }
        }

        private Task<bool> CheckLogsAsync()
        {
            return Task<bool>.Run(new Func<bool>(CheckLogs));
        }

        private void Clean()
        {
            var now = DateTime.Now.Date;
            var oldDate = now.AddDays(-1 * _days);
            using (var context = new AppsEntities())
            {
                var logs = context.Logs.Where(l => l.DateCreated < oldDate).ToList();
                var screenshots = context.Screenshots.Where(s => s.Date < oldDate).ToList();
                var usages = context.Usages.Where(u => u.UsageStart < oldDate).ToList();
                var blockedApps = context.BlockedApps.Where(b => b.Date < oldDate).ToList();

                context.Screenshots.RemoveRange(screenshots);
                context.Logs.RemoveRange(logs);
                context.Usages.RemoveRange(usages);
                context.BlockedApps.RemoveRange(blockedApps);

                context.SaveChanges();
            }

            DeleteEmptyLogs();
        }

        private Task CleanAsync()
        {
            return Task.Run(new Action(Clean));
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
                }

                context.SaveChanges();

                foreach (var app in context.Applications.Where(a => a.Windows.Count == 0))
                {
                    if (!context.Applications.Local.Any(a => a.ApplicationID == app.ApplicationID))
                        context.Applications.Attach(app);
                    context.Applications.Remove(app);
                }

                context.SaveChanges();
            }
        }

        public void Dispose()
        {
        }
    }
}
