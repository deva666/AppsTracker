#region Licence
/*
  *  Author: Marko Devcic, madevcic@gmail.com
  *  Copyright: Marko Devcic, 2015
  *  Licence: http://creativecommons.org/licenses/by-nc-nd/4.0/
 */
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;
using AppsTracker.Data.Db;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Service
{
    [Export(typeof(ICategoriesService))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public sealed class CategoriesService : ICategoriesService
    {
        private Boolean isDisposed = false;
        private AppsEntities context;

        public Aplication ReloadApp(Aplication app)
        {
            context = context ?? new AppsEntities();
            return context.Applications.First(a => a.ApplicationID == app.ApplicationID);
        }

        public List<Aplication> GetApps(int userId)
        {
            context = context ?? new AppsEntities();
            var apps = context.Applications.Where(a => a.UserID == userId);
            return apps.ToList();
        }

        public ObservableCollection<AppCategory> GetCategories(int userId)
        {
            context = context ?? new AppsEntities();
            var categories = context.AppCategories
                                    .Include(c => c.Applications)
                                    .Where(c => c.Applications.Where(a => a.UserID == userId).Any());
            foreach (var cat in categories)
            {
                cat.ObservableApplications = new ObservableCollection<Aplication>(cat.Applications);
            }
            return new ObservableCollection<AppCategory>(categories);
        }

        public void SaveChanges(IEnumerable<AppCategory> categoriesToDelete, IEnumerable<AppCategory> modifiedCategories)
        {
            context = context ?? new AppsEntities();
            foreach (var cat in categoriesToDelete)
            {
                if (cat.AppCategoryID != default(int))
                {
                    context.Entry(cat).State = EntityState.Deleted;
                }
            }

            foreach (var cat in modifiedCategories)
            {
                if (cat.AppCategoryID == default(int))
                {
                    context.Entry(cat).State = System.Data.Entity.EntityState.Added;
                }
                else
                {
                    context.Entry(cat).State = System.Data.Entity.EntityState.Modified;
                }

                SetApplications(cat);
            }

            context.SaveChanges();
        }

        private void SetApplications(AppCategory category)
        {
            category.Applications.Clear();
            foreach (var app in category.ObservableApplications)
            {
                category.Applications.Add(app);
            }
        }

        public void Dispose()
        {
            if (isDisposed == false && context != null)
            {
                context.Dispose();
                context = null;
                isDisposed = true;
            }
        }
    }
}
