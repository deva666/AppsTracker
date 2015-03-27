#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Threading;
using System.Windows.Threading;
using AppsTracker.Data.Models;

namespace AppsTracker
{
    public static class Extensions
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static void InvokeSafely<T>(this EventHandler<T> handler, object sender, T args) where T : EventArgs
        {
            var copy = Volatile.Read(ref handler);
            if (copy != null)
                copy(sender, args);
        }

        public static void InvokeSafely(this EventHandler handler, object sender, EventArgs args)
        {
            var copy = Volatile.Read(ref handler);
            if (copy != null)
                copy(sender, args);
        }

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

        public static void Reset(this DispatcherTimer timer)
        {
            timer.Stop();
            timer.Start();
        }

        public static bool Running(this LoggingStatus status)
        {
            if (status == LoggingStatus.Running)
                return true;
            else
                return false;
        }

        //public static LoggingStatus ConvertToLoggingStatus(this bool boolean)
        //{
        //    if (boolean)
        //        return LoggingStatus.Running;
        //    else
        //        return LoggingStatus.Stopped;
        //}


        //public static double ConvertToDouble(this ScreenShotInterval interval)
        //{
        //    switch (interval)
        //    {
        //        case ScreenShotInterval.TenSeconds:
        //            return 10 * Constants.MILISECONDS_IN_SECOND;
        //        case ScreenShotInterval.ThirtySeconds:
        //            return 30 * Constants.MILISECONDS_IN_SECOND;
        //        case ScreenShotInterval.OneMinute:
        //            return 60 * Constants.MILISECONDS_IN_SECOND;
        //        case ScreenShotInterval.TwoMinute:
        //            return 2 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
        //        case ScreenShotInterval.FiveMinute:
        //            return 5 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
        //        case ScreenShotInterval.TenMinute:
        //            return 10 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
        //        case ScreenShotInterval.ThirtyMinute:
        //            return 30 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
        //        case ScreenShotInterval.OneHour:
        //            return 60 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND;
        //        default:
        //            return 0;
        //    }
        //}

        //public static ScreenShotInterval ConvertToScreenshotInterval(this double miliseconds)
        //{
        //    if (miliseconds == 10 * Constants.MILISECONDS_IN_SECOND)
        //        return ScreenShotInterval.TenSeconds;
        //    else if (miliseconds == 30 * Constants.MILISECONDS_IN_SECOND)
        //        return ScreenShotInterval.ThirtySeconds;
        //    else if (miliseconds == 60 * Constants.MILISECONDS_IN_SECOND)
        //        return ScreenShotInterval.OneMinute;
        //    else if (miliseconds == 2 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
        //        return ScreenShotInterval.TwoMinute;
        //    else if (miliseconds == 5 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
        //        return ScreenShotInterval.FiveMinute;
        //    else if (miliseconds == 10 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
        //        return ScreenShotInterval.TenMinute;
        //    else if (miliseconds == 30 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
        //        return ScreenShotInterval.ThirtyMinute;
        //    else if (miliseconds == 60 * Constants.SECONDS_IN_MINUTE * Constants.MILISECONDS_IN_SECOND)
        //        return ScreenShotInterval.OneHour;
        //    return ScreenShotInterval.TwoMinute;
        //}
    }
}
