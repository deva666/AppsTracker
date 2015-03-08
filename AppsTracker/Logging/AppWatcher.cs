using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;

namespace AppsTracker.Logging
{
    internal sealed class AppWatcher
    {
        public void AppChanged(Aplication app)
        {
            //using (var context = new AppsEntities())
            //{
            //    var warnings = context.AppWarnings.Where(w => w.Applications.Any(a => a.ApplicationID == app.ApplicationID));
            //    var periods = warnings.Select(w => w.Period);
            //    var warn = warnings.FirstOrDefault();

            //    var limitReached = new TimeSpan(warn.Limit).Milliseconds - new TimeSpan(GetTodayDuration(app)).Milliseconds;
            //    System.Threading.Timer timer = new System.Threading.Timer((s) => { }, null, limitReached, -1);
                
            //}
        }

        private long GetTodayDuration(Aplication app)
        {
            return 1;
            //using (var context = new AppsEntities())
            //{
            //    var loadedApp = context.Applications.Include(a => a.Windows.Select(w => w.Logs)).First(a => a.ApplicationID == app.ApplicationID);
            //    return loadedApp.Windows.SelectMany(w => w.Logs).Where(l => l.DateCreated >= DateTime.Now.Date).Sum(l => l.Duration);
            //}
        }
    }
}
