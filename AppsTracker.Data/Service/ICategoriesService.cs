using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AppsTracker.Data.Models;
namespace AppsTracker.Data.Service
{
    public interface ICategoriesService : IDisposable, IBaseService
    {
        List<Aplication> GetApps();
        ObservableCollection<AppCategory> GetCategories();
        void SaveChanges(IEnumerable<AppCategory> categoriesToDelete, IEnumerable<AppCategory> modifiedCategories);
    }
}
