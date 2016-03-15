#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;

namespace AppsTracker.Common.Utils
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

        public static void Reset(this DispatcherTimer timer)
        {
            timer.Stop();
            timer.Start();
        }

        public static Tuple<DateTime, DateTime> GetWeekBeginAndEnd(this DateTime dateTime)
        {
            var today = DateTime.Today;
            int delta = DayOfWeek.Monday - today.DayOfWeek;
            if (delta > 0)
                delta -= 7;
            var weekBegin = today.AddDays(delta);
            var weekEnd = weekBegin.AddDays(6);
            return new Tuple<DateTime, DateTime>(weekBegin, weekEnd);
        }

        public static void SetPropertyValue(this object _object, string propertyName, object propertyValue)
        {
            var propertyInfo = _object.GetType().GetProperty(propertyName, System.Reflection.BindingFlags.NonPublic);

            if (propertyInfo != null)
                propertyInfo.SetValue(_object, propertyValue);
        }

        public static T ThrowIfNull<T>(this T argument, String argumentName) where T : class
        {
            if (argument == null)
                throw new ArgumentNullException(argumentName);

            return argument;
        }

        public static void WaitUntilEmpty<T>(this BlockingCollection<T> collection)
        {
            var timeStamp = DateTime.Now;
            while (collection.Count > 0 || DateTime.Now > timeStamp.AddSeconds(5))
            {
                Thread.Sleep(100);
            }
        }
    }
}
