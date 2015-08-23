using System;
using AppsTracker.Data.Utils;

namespace AppsTracker.Tracking.Hooks
{
    public sealed class AppChangedArgs : EventArgs
    {
        public string WindowTitle { get; private set; }
        public AppInfo AppInfo { get; private set; }

        public LogInfo LogInfo { get; private set; }

        public AppChangedArgs(string windowTitle, AppInfo appInfo)
        {
            AppInfo = appInfo;
            WindowTitle = windowTitle;
        }

        public AppChangedArgs(LogInfo logInfo)
        {
            LogInfo = logInfo;
        }
    }
}
