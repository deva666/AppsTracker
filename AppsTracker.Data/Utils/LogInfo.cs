using System;
using System.Collections.Generic;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Utils
{
    public struct LogInfo
    {
        private bool isFinished;
        public bool IsFinished { get { return isFinished; } }

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

        private HashSet<Screenshot> screenshots;
        public ICollection<Screenshot> Screenshots
        {
            get
            {
                return screenshots ?? (screenshots = new HashSet<Screenshot>());
            }
        }

        public static LogInfo EmptyLog { get { return new LogInfo(true); } }

        private LogInfo(bool finished)
            : this(null, null)
        {
            isFinished = finished;
        }

        public LogInfo(AppInfo _appInfo, String _windowTitle)
        {
            start = DateTime.Now;
            utcStart = DateTime.UtcNow;
            screenshots = null;
            appInfo = _appInfo;
            windowTitle = _windowTitle;
            isFinished = false;
            end = DateTime.MinValue;
            utcEnd = DateTime.MinValue;
        }

        public void Finish()
        {
            end = DateTime.Now;
            utcEnd = DateTime.UtcNow;
            isFinished = true;
        }

        public static bool operator ==(LogInfo first, LogInfo second)
        {
            return first.AppInfo == second.AppInfo
                && first.WindowTitle == second.WindowTitle
                && first.Start == second.Start
                && first.End == second.End
                && first.IsFinished == first.IsFinished;
        }

        public static bool operator !=(LogInfo first, LogInfo second)
        {
            return !(first.AppInfo == second.AppInfo
                && first.WindowTitle == second.WindowTitle
                && first.Start == second.Start
                && first.End == second.End
                && first.IsFinished == first.IsFinished);
        }
    }
}
