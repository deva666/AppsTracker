using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Entity;
using System.Threading.Tasks;
using AppsTracker.Models.EntityModels;
using AppsTracker.DAL;
using AppsTracker.Models.Proxy;
using System.Linq.Expressions;

namespace AppsTracker.Service
{
    public sealed class AppsService : IAppsService, IDisposable
    {
        private bool _disposed;
        private AppsEntities _context = new AppsEntities();

        public IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter) where T : class
        {
            return _context.Set<T>().AsNoTracking().Where(filter).ToList();
        }

        public IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class
        {
            var query = _context.Set<T>();
            foreach (var nav in navigations)
                query.Include(nav);
            return query.AsNoTracking().Where(filter).ToList();
        }

        public IQueryable<T> GetQueryable<T>() where T : class
        {
            return _context.Set<T>().AsNoTracking();
        }

        public Log CreateNewLog(string windowTitle, int usageID, int userID, IAppInfo appInfo, out bool newApp)
        {
            newApp = false;

            Aplication app = GetSingle<Aplication>(a => a.UserID == userID
                                                    && a.Name == appInfo.ProcessName);

            if (app == null)
            {
                app = new Aplication(appInfo.ProcessName,
                                                appInfo.ProcessFileName,
                                                appInfo.ProcessVersion,
                                                appInfo.ProcessDescription,
                                                appInfo.ProcessCompany,
                                                appInfo.ProcessRealName) { UserID = userID };
                newApp = true;
                _context.Applications.Add(app);

            }
            Window window = GetSingle<Window>(w => w.Title == windowTitle
                                                 && w.ApplicationID == app.ApplicationID);

            if (window == null)
            {
                window = new Window(windowTitle) { Application = app };
                _context.Windows.Add(window);
            }

            _context.SaveChanges();

            return new Log(window.WindowID, usageID);
        }

        public T GetSingle<T>(Expression<Func<T, bool>> filter) where T : class
        {
            return _context.Set<T>().AsNoTracking().Where(filter).FirstOrDefault();
        }

        public T GetSingle<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class
        {
            var query = _context.Set<T>();
            foreach (var nav in navigations)
                query.Include(nav);
            return query.AsNoTracking().Where(filter).FirstOrDefault();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public void Add<T>(T item) where T : class
        {
            _context.Set<T>().Add(item);
        }

        public void Save()
        {
            _context.SaveChanges();
        }


    }
}
