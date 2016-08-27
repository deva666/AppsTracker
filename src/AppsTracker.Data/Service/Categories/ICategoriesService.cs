using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AppsTracker.Data.Models;

namespace AppsTracker.Data.Repository
{
    public interface ICategoriesService : IDisposable, IBaseService
    {
        Aplication ReloadApp(Aplication app);

        List<Aplication> GetApps(int userId);
       
        ObservableCollection<AppCategory> GetCategories(int userId);
      
        Task SaveChangesAsync(IEnumerable<AppCategory> categoriesToDelete, IEnumerable<AppCategory> modifiedCategories);
    }
}
