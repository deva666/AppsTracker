using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using AppsTracker.Models.EntityModels;
using AppsTracker.Models.Proxy;

namespace AppsTracker.DAL.Service
{
    public interface IAppsService : IBaseService
    {
        Log CreateNewLog(string windowTitle, int usageID, int userID, IAppInfo appInfo, out bool newApp);
        Uzer InitUzer(string userName);
        Usage InitLogin(int userID);

        IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter) where T : class;
        IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class;
        IQueryable<T> GetQueryable<T>() where T : class;
        T GetSingle<T>(Expression<Func<T, bool>> filter) where T : class;
        T GetSingle<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class;
        void Add<T>(T item) where T : class;     

        Task<IEnumerable<T>> GetFilteredAsync<T>(Expression<Func<T, bool>> filter) where T : class;
        Task<IEnumerable<T>> GetFilteredAsync<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class;
        Task<T> GetSingleAsync<T>(Expression<Func<T, bool>> filter) where T : class;
        Task<T> GetSingleAsync<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class;
        Task AddAsync<T>(T item) where T : class;
    }
}
