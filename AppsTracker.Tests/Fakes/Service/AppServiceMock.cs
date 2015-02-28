#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2014
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using AppsTracker.Data.Models;
using AppsTracker.Data.Service;

namespace AppsTracker.Tests.Fakes.Service
{
    public class AppsServiceMock : IAppsService
    {
        public IEnumerable<T> GetFiltered<T>(System.Linq.Expressions.Expression<Func<T, bool>> filter) where T : class
        {
            System.Threading.Thread.Sleep(500);
            return new List<T>();
        }

        public IEnumerable<T> GetFiltered<T>(System.Linq.Expressions.Expression<Func<T, bool>> filter, params System.Linq.Expressions.Expression<Func<T, object>>[] navigations) where T : class
        {
            System.Threading.Thread.Sleep(500);
            return new List<T>();
        }

        public int DeleteScreenshots(IEnumerable<Log> logs)
        {
            return 1;
        }
    }
}
