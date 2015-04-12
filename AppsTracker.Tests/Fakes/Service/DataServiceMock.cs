#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using AppsTracker.Data.Models;
using AppsTracker.Service;

namespace AppsTracker.Tests.Fakes.Service
{
    [Export(typeof(IDataService))]
    public class DataServiceMock : IDataService
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

        public int DeleteScreenshotsInLogs(IEnumerable<Log> logs)
        {
            return 1;
        }

        public IEnumerable<T> Get<T>() where T : class
        {
            System.Threading.Thread.Sleep(500);
            return new List<T>();
        }

        public IEnumerable<T> Get<T>(params System.Linq.Expressions.Expression<Func<T, object>>[] navigations) where T : class
        {
            System.Threading.Thread.Sleep(500);
            return new List<T>();
        }


        public int DeleteOldScreenshots(int daysBackwards)
        {
            return 0;
        }

        public void DeleteOldLogs(int daysTreshold)
        {

        }


        System.Threading.Tasks.Task IDataService.DeleteScreenshotsInLogs(IEnumerable<Log> logs)
        {
            return System.Threading.Tasks.Task.Delay(100);
        }

        public System.Threading.Tasks.Task DeleteScreenshots(IEnumerable<Screenshot> screenshots)
        {
            return System.Threading.Tasks.Task.Delay(100);
        }
    }
}
