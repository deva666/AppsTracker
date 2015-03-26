#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using AppsTracker.Common.Utils;
using System;
using System.Collections.Generic;

namespace AppsTracker.Data.Service
{
    public sealed class ServiceFactory
    {
        private static readonly Dictionary<Type, Func<IBaseService>> serviceMap = new Dictionary<Type, Func<IBaseService>>();

        private ServiceFactory() { }

        public static void Register<T>(Func<T> valueFactory) where T : class, IBaseService
        {
            Ensure.NotNull(valueFactory);
            if (serviceMap.ContainsKey(typeof(T)))
                return;

            serviceMap.Add(typeof(T), valueFactory);
        }

        public static T Get<T>() where T : class, IBaseService
        {
            Func<IBaseService> valueFactory;
            var success = serviceMap.TryGetValue(typeof(T), out valueFactory);
            if (success == false)
                throw new InvalidOperationException(string.Format("Can't resolve {0}", typeof(T)));

            return (T)valueFactory();
        }

        public static bool ContainsKey<T>() where T : class, IBaseService
        {
            return serviceMap.ContainsKey(typeof(T));
        }
    }
}
