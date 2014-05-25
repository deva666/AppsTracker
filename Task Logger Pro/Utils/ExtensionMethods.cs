using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using AppsTracker.Models.EntityModels;

namespace Task_Logger_Pro
{
    public static class ExtensionMethods
    {
        public static string ToExtendedString(this UsageTypes type)
        {
            switch (type)
            {
                case UsageTypes.Login:
                    return "USER LOGGED IN";
                case UsageTypes.Idle:
                    return "USER IDLE";
                case UsageTypes.Locked:
                    return "SYSTEM LOCKED";
                case UsageTypes.Stopped:
                    return "LOGGING STOPPED";
                default:
                    return string.Empty;
            }
        }

        public static bool CheckIfAppExists(this DbSet<Aplication> collection, Aplication item, Uzer uzer)
        {
            var app = from a in collection
                      where a.Name == item.Name & a.UserID == uzer.UserID
                      select a;
            return app.Count() > 0;
        }

        public static bool CheckIfAppExists(this DbSet<Aplication> collection, string appName, Uzer uzer)
        {
            var app = from a in collection
                      where a.Name == appName & a.UserID == uzer.UserID
                      select a;
            return app.Count() > 0;
        }

        public static bool CheckIfWindowExists(this DbSet<Window> collection, Window item, int appID)
        {
            var window = from w in collection
                         where w.ApplicationID == appID & w.Title == item.Title
                         select w;
            return window.Count() > 0;
        }

        public static bool CheckIfWindowExists(this DbSet<Window> collection, string windowTitle, int appID)
        {
            var window = from w in collection
                         where w.ApplicationID == appID & w.Title == windowTitle
                         select w;
            return window.Count() > 0;
        }

        public static bool CheckIfUserExists(this DbSet<Uzer> collection, string username)
        {
            if (collection.Count(a => a.Name == username) == 0) return false;
            else return true;
        }

        public static TimeSpan GetAllAppDuration(this DbSet<Aplication> collection)
        {
            TimeSpan duration = new TimeSpan();
            foreach (var item in collection)
            {
                duration += item.Windows.GetAllWindowDuration();
            }
            return duration;
        }

        public static TimeSpan GetAllWindowDuration(this DbSet<Window> collection)
        {
            TimeSpan duration = new TimeSpan();
            foreach (var window in collection)
            {
                foreach (var log in window.Logs)
                {
                    duration += TimeSpan.FromTicks(log.Duration);
                }
            }
            return duration;
        }

        public static TimeSpan GetAllWindowDuration(this IEnumerable<Window> collection)
        {
            TimeSpan duration = new TimeSpan();
            foreach (var window in collection)
            {
                foreach (var log in window.Logs)
                {
                    duration += TimeSpan.FromTicks(log.Duration);
                }
            }
            return duration;
        }

        public static TimeSpan GetAppDuration(this Aplication aplication)
        {
            TimeSpan duration = new TimeSpan();
            foreach (var window in aplication.Windows)
            {
                foreach (var log in window.Logs)
                {
                    duration += TimeSpan.FromTicks(log.Duration);
                }
            }
            return duration;
        }

        public static TimeSpan GetWindowDuration(this Window window)
        {
            TimeSpan duration = new TimeSpan();
            foreach (var log in window.Logs)
            {
                duration += TimeSpan.FromTicks(log.Duration);
            }
            return duration;
        }

        public static void Reset(this System.Timers.Timer timer)
        {
            timer.Stop(); timer.Start();
        }

        public static bool ItemExists(this DbSet<AppsToBlock> appsToBlock, Uzer uzer, Aplication aplication)
        {
            return (from a in appsToBlock
                    where a.UserID == uzer.UserID
                    && a.ApplicationID == aplication.ApplicationID
                    select a).Count() > 0;
        }

        public static void Reset(this DispatcherTimer timer)
        {
            timer.Stop();
            timer.Start();
        }

        public static bool Running(this LoggingStatus status)
        {
            if (status == LoggingStatus.Running) return true;
            else return false;
        }

        public static LoggingStatus ConvertToLoggingStatus(this bool boolean)
        {
            if (boolean) return LoggingStatus.Running;
            else return LoggingStatus.Stopped;
        }


        public static double ConvertToDouble(this ScreenShotInterval interval)
        {
            switch (interval)
            {
                case ScreenShotInterval.TenSeconds:
                    return 10 * Constants.MILISECONDS_IN_SECOND;
                case ScreenShotInterval.ThirtySeconds:
                    return 30 * Constants.MILISECONDS_IN_SECOND;
                case ScreenShotInterval.OneMinute:
                    return 60 * Constants.MILISECONDS_IN_SECOND;
                case ScreenShotInterval.TwoMinute:
                    return 2 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
                case ScreenShotInterval.FiveMinute:
                    return 5 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
                case ScreenShotInterval.TenMinute:
                    return 10 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
                case ScreenShotInterval.ThirtyMinute:
                    return 30 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
                case ScreenShotInterval.OneHour:
                    return 60 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
                default:
                    return 0;
            }
        }

        public static double CovertToDouble(this EmailInterval interval)
        {
            switch (interval)
            {
                case EmailInterval.FiveMinute:
                    return 5 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
                case EmailInterval.TenMinute:
                    return 10 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
                case EmailInterval.ThirtyMinute:
                    return 30 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
                case EmailInterval.OneHour:
                    return 60 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
                case EmailInterval.TwoHour:
                    return 120 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
                default:
                    return 0;
            }
        }

        public static EmailInterval ConvertToEmailInterval(this double miliseconds)
        {

            if (miliseconds == 5 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
                return EmailInterval.FiveMinute;
            else if (miliseconds == 10 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
                return EmailInterval.TenMinute;
            else if (miliseconds == 30 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
                return EmailInterval.ThirtyMinute;
            else if (miliseconds == 60 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
                return EmailInterval.OneHour;
            else if (miliseconds == 120 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
                return EmailInterval.TwoHour;

            return EmailInterval.OneHour;

        }

        public static ScreenShotInterval ConvertToScreenshotInterval(this double miliseconds)
        {
            if (miliseconds == 10 * Constants.MILISECONDS_IN_SECOND)
                return ScreenShotInterval.TenSeconds;
            else if (miliseconds == 30 * Constants.MILISECONDS_IN_SECOND)
                return ScreenShotInterval.ThirtySeconds;
            else if (miliseconds == 60 * Constants.MILISECONDS_IN_SECOND)
                return ScreenShotInterval.OneMinute;
            else if (miliseconds == 2 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
                return ScreenShotInterval.TwoMinute;
            else if (miliseconds == 5 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
                return ScreenShotInterval.FiveMinute;
            else if (miliseconds == 10 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
                return ScreenShotInterval.TenMinute;
            else if (miliseconds == 30 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
                return ScreenShotInterval.ThirtyMinute;
            else if (miliseconds == 60 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
                return ScreenShotInterval.OneHour;
            return ScreenShotInterval.TwoMinute;
        }
    }
}
