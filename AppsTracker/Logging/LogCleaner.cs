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

using AppsTracker.Models.Proxy;

namespace AppsTracker.Logging
{
    internal class LogCleaner : IComponent
    {
        LazyInit<LogCleanerHelper> _cleaner;

        public LogCleaner(ISettings settings)
        {
            _cleaner = new LazyInit<LogCleanerHelper>(() => new LogCleanerHelper(settings.OldLogDeleteDays));
            _cleaner.Enabled = settings.DeleteOldLogs;
        }

        public void SettingsChanged(ISettings settings)
        {
            _cleaner.Enabled = settings.DeleteOldLogs;
            if (_cleaner.Enabled)
                _cleaner.Component.Days = settings.OldLogDeleteDays;
        }

        public void SetComponentEnabled(bool enabled)
        {
            _cleaner.Enabled = enabled;
        }

        public void Dispose()
        {            
        }
    }
}
