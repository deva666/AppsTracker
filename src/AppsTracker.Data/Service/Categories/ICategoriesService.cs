using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Service
{
    public interface ICategoriesService : IDisposable, IBaseService
    {
        Aplication ReloadApp(Aplication app);

        List<Aplication> GetApps(int userId);
       
        ObservableCollection<AppCategory> GetCategories(int userId);
      
        void SaveChanges(IEnumerable<AppCategory> categoriesToDelete, IEnumerable<AppCategory> modifiedCategories);
    }
}
