using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using AppsTracker.Models.EntityModels;
using AppsTracker.Models.Proxy;

namespace AppsTracker.DAL.Service
{
    public sealed class AppsService : IAppsService
    {
        public IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter) where T : class
        {
            using (var context = new AppsEntities())
            {
                return context.Set<T>().AsNoTracking().Where(filter).ToList();
            }
        }

        public IEnumerable<T> GetFiltered<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class
        {
            using (var context = new AppsEntities())
            {
                var query = context.Set<T>();
                foreach (var nav in navigations)
                    query.Include(nav);
                return query.AsNoTracking().Where(filter).ToList();
            }
        }

        public IQueryable<T> GetQueryable<T>() where T : class
        {
            using (var context = new AppsEntities())
            {
                return context.Set<T>().AsNoTracking();
            }
        }

        public Log CreateNewLog(string windowTitle, int usageID, int userID, IAppInfo appInfo, out bool newApp)
        {
            using (var context = new AppsEntities())
            {
                newApp = false;

                Aplication app = context.Applications.FirstOrDefault(a => a.UserID == userID
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
                }

                Window window = context.Windows.FirstOrDefault(w => w.Title == windowTitle
                                                     && w.ApplicationID == app.ApplicationID);

                if (window == null)
                {
                    window = new Window(windowTitle) { Application = app };
                    context.Windows.Add(window);
                }

                context.SaveChanges();

                return new Log(window.WindowID, usageID);
            }
        }

        public T GetSingle<T>(Expression<Func<T, bool>> filter) where T : class
        {
            using (var context = new AppsEntities())
            {
                return context.Set<T>().AsNoTracking().Where(filter).FirstOrDefault();
            }
        }

        public T GetSingle<T>(Expression<Func<T, bool>> filter, params Expression<Func<T, object>>[] navigations) where T : class
        {
            using (var context = new AppsEntities())
            {
                var query = context.Set<T>();
                foreach (var nav in navigations)
                    query.Include(nav);
                return query.AsNoTracking().Where(filter).FirstOrDefault();
            }
        }

        public void Add<T>(T item) where T : class
        {
            using (var context = new AppsEntities())
            {
                context.Set<T>().Add(item);
                context.SaveChanges();
            }
        }

        public Task AddAsync<T>(T item) where T : class
        {
            using (var context = new AppsEntities())
            {
                context.Set<T>().Add(item);
                return context.SaveChangesAsync();
            }
        }


        public Uzer InitUzer(string userName)
        {
            using (var context = new AppsEntities())
            {
                Uzer user = context.Users.FirstOrDefault(u => u.Name == userName);

                if (user == null)
                {
                    user = new Uzer() { Name = userName };
                    context.Users.Add(user);
                    context.SaveChanges();
                }

                return user;
            }
        }

        public Usage InitLogin()
        {
            throw new NotImplementedException();
        }
    }
}
