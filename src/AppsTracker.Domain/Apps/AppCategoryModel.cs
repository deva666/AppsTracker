using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AppsTracker.Data.Models;

namespace AppsTracker.Domain.Apps
{
    public sealed class AppCategoryModel
    {
        public AppCategoryModel(string name)
        {
            Name = name;
            ObservableApplications = new ObservableCollection<AppModel>();
        }

        public AppCategoryModel(AppCategory category)
        {
            ID = category.ID;
            Name = category.Name;
        }

        public int ID { get; }

        public string Name { get; }

        public ObservableCollection<AppModel> ObservableApplications { get; private set; }

        public IEnumerable<AppModel> Applications { get; private set; }

        public void SetApps(IEnumerable<Aplication> apps)
        {
            Applications = apps.Select(a => new AppModel(a));
            ObservableApplications = new ObservableCollection<AppModel>(Applications);
        }
    }
}