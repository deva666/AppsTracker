using System;
using System.Collections.Generic;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Utils
{
    public sealed class LogInfo
    {
        private bool isFinished;
        public bool IsFinished { get { return isFinished; } }

        public bool HasScreenshots { get { return screenshots != null; } }

        private Guid guid;
        public Guid Guid { get { return guid; } }

        private Log log;
        public Log Log
        {
            get { return log; }
            set { log = value; }
        }

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

        private readonly static LogInfo emptyLog = new LogInfo(true);
        public static LogInfo Empty { get { return emptyLog; } }

        private LogInfo(bool finished)
            : this(AppInfo.Empty, string.Empty)
        {
            isFinished = finished;
        }


        public static LogInfo Create(AppInfo _appInfo, String _windowTitle)
        {
            return new LogInfo(_appInfo, _windowTitle);
        }

        private LogInfo(AppInfo _appInfo, String _windowTitle)
        {
            start = end = DateTime.Now;
            utcStart = utcEnd = DateTime.UtcNow;
            screenshots = null;
            log = null;
            appInfo = _appInfo;
            windowTitle = _windowTitle;
            isFinished = false;
            guid = Guid.NewGuid();
        }

        public void Finish()
        {
            end = DateTime.Now;
            utcEnd = DateTime.UtcNow;
            isFinished = true;
        }
    }
}
