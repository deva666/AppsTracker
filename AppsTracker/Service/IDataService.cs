#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using AppsTracker.Data.Models;

namespace AppsTracker.Service
{
    public interface IDataService : IBaseService
    {
        IEnumerable<T> Get<T>() where T : class;
      
        IEnumerable<T> Get<T>(params Expression<Func<T, object>>[] navigations) where T : class;
        
        IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter) where T : class;
       
        IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class;
       
        int DeleteScreenshotsInLogs(IEnumerable<Log> logs);
       
        int DeleteOldScreenshots(int daysBackwards);
       
        void DeleteOldLogs(int daysTreshold);
    }
}
