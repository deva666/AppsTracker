using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppsTracker.MVVM
{
    public static class MediatorMessages
    {
        public const string FilterClearedMessage = "Clear Filter";
        public const string ApplicationAdded = "New Application added";
        public const string RefreshLogs = "Refresh logs";
        public const string AppsToBlockChanged = "Changing AppsToBlock";
        public const string STOP_LOGGING= "Stop logging";
        public const string RESUME_LOGGING = "Resume logging";
    }
}
