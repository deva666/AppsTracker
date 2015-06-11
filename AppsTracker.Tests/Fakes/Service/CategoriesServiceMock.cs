using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Service;

namespace AppsTracker.Tests.Fakes.Service
{
    [Export(typeof(ICategoriesService))]
    class CategoriesServiceMock : ICategoriesService
    {
        public List<Data.Models.Aplication> GetApps()
        {
            return new List<Data.Models.Aplication>();
        }

        public System.Collections.ObjectModel.ObservableCollection<Data.Models.AppCategory> GetCategories()
        {
            return new System.Collections.ObjectModel.ObservableCollection<Data.Models.AppCategory>();
        }

        public void SaveChanges(IEnumerable<Data.Models.AppCategory> categoriesToDelete, IEnumerable<Data.Models.AppCategory> modifiedCategories)
        {
            
        }

        public void Dispose()
        {
            
        }

        public Data.Models.Aplication ReloadApp(Data.Models.Aplication app)
        {
            throw new NotImplementedException();
        }
    }
}
