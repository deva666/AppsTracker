#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Threading.Tasks;

using AppsTracker.Common.Utils;
using AppsTracker.Data.Service;

namespace AppsTracker.Logging
{
    internal sealed class LogCleanerHelper : IDisposable
    {
        private int daysTreshold;

        private readonly IDataService dataService;

        public int Days
        {
            get
            {
                return daysTreshold;
            }
            set
            {
                Ensure.Condition<InvalidOperationException>(value >= 15, "Minimum value must be 15");
                daysTreshold = value;
            }
        }

        public LogCleanerHelper(int daysToDelete)
        {
            dataService = ServiceFactory.Get<IDataService>();
            Days = daysToDelete;
            CleanAsync();
        }

        private void Clean()
        {
            dataService.DeleteOldLogs(daysTreshold);
        }

        private Task CleanAsync()
        {
            return Task.Run(new Action(Clean));
        }

        public void Dispose()
        {
        }
    }
}
