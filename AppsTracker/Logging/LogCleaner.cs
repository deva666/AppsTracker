#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.Data.Models;

namespace AppsTracker.Logging
{
    internal class LogCleaner : IComponent
    {
        LazyInit<LogCleanerHelper> _cleaner;

        public LogCleaner(Setting settings)
        {
            _cleaner = new LazyInit<LogCleanerHelper>(() => new LogCleanerHelper(settings.OldLogDeleteDays));
            _cleaner.Enabled = settings.DeleteOldLogs;
        }

        public void SettingsChanged(Setting settings)
        {
            _cleaner.Enabled = settings.DeleteOldLogs;
            if (_cleaner.Enabled)
                _cleaner.Component.Days = settings.OldLogDeleteDays;
        }

        public void Dispose()
        {
        }
    }
}
