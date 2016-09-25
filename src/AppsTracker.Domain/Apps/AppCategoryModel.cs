using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace AppsTracker.Domain.Apps
{
    public sealed class AppCategoryModel
    {
        public ObservableCollection<AppModel> ObservableApplications { get; set; }

        public AppCategoryModel()
        {
        }

        public int ID { get; private set; }

        public string Name { get; private set; }

        public ICollection<AppModel> Applications { get; set; }
    }
}