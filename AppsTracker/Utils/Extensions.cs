#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;

namespace AppsTracker
{
    public static class Extensions
    {
        public static void ForEachAction<T>(this IEnumerable<T> collection, Action<T> action)
        {
            Ensure.NotNull(action, "action");
            foreach (var item in collection)
            {
                action(item);
            }
        }

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
                    return "TRACKING STOPPED";
                default:
                    return string.Empty;
            }
        }

        public static void Reset(this DispatcherTimer timer)
        {
            timer.Stop();
            timer.Start();
        }

        public static void AttachToContextAsModified<T>(this IEnumerable<T> collection, System.Data.Entity.DbContext context) where T : class
        {
            Ensure.NotNull(context, "context");

            foreach (var item in collection)
            {
                context.Entry(item).State = System.Data.Entity.EntityState.Modified;
            }
        }
    }
}
