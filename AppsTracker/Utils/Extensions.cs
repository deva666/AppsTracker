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
    }
}
