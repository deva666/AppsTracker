﻿
namespace AppsTracker.Common.Communication
{
    public static class MediatorMessages
    {
        public const string FILTER_CLEARED = "Clear Filter";
        public const string APPLICATION_ADDED = "New Application added";
        public const string REFRESH_LOGS = "Refresh logs";
        public const string STOP_TRACKING = "Stop logging";
        public const string RESUME_TRACKING = "Resume logging";
        public const string RELOAD_SETTINGS = "Reload settings";
        public const string APP_LIMIT_REACHED = "app limit reached";
        public const string APP_LIMITS_CHANGIING = "app limits changing";
        public const string TRACKING_ENABLED_CHANGING = "tracking enabled changing";
        public const string SCREENSHOT_TAKEN = "screenshot taken";
    }
}
