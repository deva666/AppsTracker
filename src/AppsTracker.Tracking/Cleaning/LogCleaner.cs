#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Service;
using AppsTracker.Tracking.Helpers;

namespace AppsTracker.Tracking
{
    [Export(typeof(ILogCleaner))]
    public sealed class LogCleaner : ILogCleaner
    {
        private int daysTreshold;

        private readonly IDataService dataService;
        private readonly IWorkQueue workQueue;

        public int Days
        {
            get { return daysTreshold; }
            set
            {
                Ensure.Condition<InvalidOperationException>(value >= 15, "Minimum value must be 15");
                daysTreshold = value;
            }
        }

        [ImportingConstructor]
        public LogCleaner(IDataService dataService,
                          IWorkQueue workQueue)
        {
            this.dataService = dataService;
            this.workQueue = workQueue;
        }

        public void Clean()
        {
            workQueue.EnqueueWork(() => dataService.DeleteOldLogs(daysTreshold));
        }

        public void Dispose()
        {
        }
    }
}
