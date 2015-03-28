#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.ComponentModel.Composition;
using AppsTracker.Data.Models;

namespace AppsTracker.Logging
{
    [Export(typeof(IComponent))]
    internal class LogCleaner : IComponent
    {
        LazyInit<LogCleanerHelper> cleaner;

        public void InitializeComponent(Setting settings)
        {
            cleaner = new LazyInit<LogCleanerHelper>(() => new LogCleanerHelper(settings.OldLogDeleteDays));
            cleaner.Enabled = settings.DeleteOldLogs;
        }


        public void SettingsChanged(Setting settings)
        {
            cleaner.Enabled = settings.DeleteOldLogs;
            if (cleaner.Enabled)
                cleaner.Component.Days = settings.OldLogDeleteDays;
        }

        public void Dispose()
        {
        }
    }
}
