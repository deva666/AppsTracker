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
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Repository
{
    public interface IRepository
    {
        event EventHandler DbSizeCritical;

        bool DBSizeOperational { get; }

        decimal GetDBSize();

        Task<decimal> GetDBSizeAsync();

        IEnumerable<T> Get<T>() where T : class;

        IEnumerable<T> Get<T>(params Expression<Func<T, object>>[] navigations) where T : class;

        IEnumerable<T> GetOrdered<T, TKey>(Expression<Func<T, bool>> filter,
                                           Expression<Func<T, TKey>> selector, 
                                           int count) where T : class;

        IEnumerable<T> GetOrderedDesc<T, TKey>(Expression<Func<T, bool>> filter,
                                               Expression<Func<T, TKey>> selector,
                                               int count) where T : class;

        IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter) where T : class;

        IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter, 
                                      params Expression<Func<T, object>>[] navigations) where T : class;

        Task<List<T>> GetFilteredAsync<T>(Expression<Func<T, bool>> filter) where T : class;

        Task<List<T>> GetFilteredAsync<T>(Expression<Func<T, bool>> filter, 
                                          params Expression<Func<T, object>>[] navigations) where T : class;

        void SaveModifiedEntity<T>(T item) where T : class;

        Task SaveModifiedEntityAsync<T>(T item) where T : class;

        void SaveNewEntity<T>(T item) where T : class;

        Task SaveNewEntityAsync<T>(T item) where T : class;

        void SaveModifiedEntityRange<T>(IEnumerable<T> items) where T : class;

        Task SaveModifiedEntityRangeAsync<T>(IEnumerable<T> items) where T : class;

        void SaveNewEntityRange<T>(IEnumerable<T> items) where T : class;

        Task SaveNewEntityRangeAsync<T>(IEnumerable<T> items) where T : class;

        void DeleteEntityRange<T>(IEnumerable<T> range) where T : class;

        Task DeleteEntityRangeAsync<T>(IEnumerable<T> range) where T : class;

        Task<int> DeleteOldScreenshotsAsync(int daysBackwards);

        Task DeleteOldLogsAsync(int daysTreshold);

        Task DeleteScreenshots(IEnumerable<Screenshot> screenshots);

        Task DeleteScreenshotsById(IEnumerable<Int32> ids);

        void CheckUnfinishedEntries();
    }
}
