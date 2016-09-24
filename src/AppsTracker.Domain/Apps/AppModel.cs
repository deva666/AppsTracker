using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppsTracker.Data.Models;
using AppsTracker.Domain.Windows;

namespace AppsTracker.Domain.Apps
{
    public sealed class AppModel
    {
        internal AppModel(Aplication aplication)
        {
            ApplicationID = aplication.ID;
            Name = aplication.Name;
            FileName = aplication.FileName;
            Version = aplication.Version;
            Description = aplication.Description;
            Company = aplication.Company;
            WinName = aplication.WinName;

            if (aplication.Limits != null)
            {
                Limits = aplication.Limits.Select(l => new AppLimitModel(l));
                ObservableLimits = new ObservableCollection<AppLimitModel>(Limits);    
            }
        }

        public ObservableCollection<AppLimitModel> ObservableLimits
        {
            get;
            set;
        }

        public int ApplicationID { get; }

        public string Name { get; }

        public string FileName { get; }

        public string Version { get; }

        public string Description { get; }

        public string Company { get; }

        public string WinName { get; }

        public IEnumerable<AppLimitModel> Limits { get; private set; }
        //public UzerModel User { get; private set; }
        //public ICollection<WindowModel> Windows { get; private set; }
        //public ICollection<AppCategoryModel> Categories { get; private set; }
    }
}
