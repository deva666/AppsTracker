#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.ComponentModel.Composition;
using AppsTracker.Data.Models;
using AppsTracker.Tracking.Helpers;

namespace AppsTracker.Tracking
{
    [Export(typeof(IComponent))]
    internal sealed class DataCleaner : IComponent, IDisposable
    {
        private ILogCleaner logCleanerInstance;

        private LazyInit<ILogCleaner> logCleaner;

        [ImportingConstructor]
        public DataCleaner(ILogCleaner logCleaner)
        {
            this.logCleanerInstance = logCleaner;
        }


        public void InitializeComponent(Setting settings)
        {
            logCleaner = new LazyInit<ILogCleaner>(() => logCleanerInstance, l => l.CleanAsync());

            logCleanerInstance.Days = settings.OldLogDeleteDays;
            logCleaner.Enabled = settings.DeleteOldLogs;
        }


        public void SettingsChanged(Setting settings)
        {
            logCleanerInstance.Days = settings.OldLogDeleteDays;
            logCleaner.Enabled = settings.DeleteOldLogs;
        }

        public void Dispose()
        {
        }
    }
}
