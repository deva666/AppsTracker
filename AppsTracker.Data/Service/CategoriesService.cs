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
    public sealed class CategoriesService : ICategoriesService
    {
        private Boolean isDisposed = false;
        private readonly AppsEntities context;


        public CategoriesService()
        {
            context = new AppsEntities();
        }

        public List<Aplication> GetApps()
        {
            var apps = context.Applications;
            return apps.ToList();
        }

        public ObservableCollection<AppCategory> GetCategories()
        {
            var categories = context.AppCategories.Include(c => c.Applications);
            foreach (var cat in categories)
            {
                cat.ObservableApplications = new ObservableCollection<Aplication>(cat.Applications);
            }
            return new ObservableCollection<AppCategory>(categories);
        }

        public void SaveChanges(IEnumerable<AppCategory> categoriesToDelete, IEnumerable<AppCategory> modifiedCategories)
        {
            foreach (var cat in categoriesToDelete)
            {
                context.Entry(cat).State = EntityState.Deleted;
            }

            foreach (var cat in modifiedCategories)
            {
                if (cat.AppCategoryID == default(int))
                    context.Entry(cat).State = System.Data.Entity.EntityState.Added;
                else
                    context.Entry(cat).State = System.Data.Entity.EntityState.Modified;

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
            if (isDisposed == false)
            {
                context.Dispose();
                isDisposed = true;
            }
        }
    }
}
