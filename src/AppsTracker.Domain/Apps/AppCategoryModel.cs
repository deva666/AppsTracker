using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AppsTracker.Data.Models;

namespace AppsTracker.Domain.Apps
{
    public sealed class AppCategoryModel
    {
        public ObservableCollection<AppModel> ObservableApplications { get; }

        public AppCategoryModel(string name)
        {
            Name = name;
            ObservableApplications = new ObservableCollection<AppModel>();
        }

        public AppCategoryModel(AppCategory category)
        {
            ID = category.ID;
            Name = category.Name;
            Applications = category.Applications.Select(a => new AppModel(a));
            ObservableApplications = new ObservableCollection<AppModel>(Applications);
        }

        public int ID { get; }

        public string Name { get; }

        public IEnumerable<AppModel> Applications { get; }
    }
}