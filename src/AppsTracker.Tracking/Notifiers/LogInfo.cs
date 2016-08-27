using System;

namespace AppsTracker.Data.Utils
{
    public sealed class LogInfo
    {
        private DateTime start;
        public DateTime Start { get { return start; } }

        private DateTime end;
        public DateTime End { get { return end; } }

        private DateTime utcStart;
        public DateTime UtcStart { get { return utcStart; } }

        private DateTime utcEnd;
        public DateTime UtcEnd { get { return utcEnd; } }

        private AppInfo appInfo;
        public AppInfo AppInfo { get { return appInfo; } }

        private String windowTitle;
        public String WindowTitle { get { return windowTitle; } }


        private readonly static LogInfo emptyLog = new LogInfo(true);
        public static LogInfo Empty { get { return emptyLog; } }

        private LogInfo(bool finished)
            : this(AppInfo.Empty, string.Empty)
        {
        }

        public static LogInfo Create(AppInfo _appInfo, String _windowTitle)
        {
            return new LogInfo(_appInfo, _windowTitle);
        }

        private LogInfo(AppInfo _appInfo, String _windowTitle)
        {
            start = end = DateTime.Now;
            utcStart = utcEnd = DateTime.UtcNow;
            appInfo = _appInfo;
            windowTitle = _windowTitle;
        }

        public void Finish()
        {
            end = DateTime.Now;
            utcEnd = DateTime.UtcNow;
        }
    }
}
