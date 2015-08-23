#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System.Collections.Generic;
using AppsTracker.Common.Utils;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Utils
{
    public static class Extensions
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
