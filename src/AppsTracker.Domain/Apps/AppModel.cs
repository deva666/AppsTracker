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
        }

        //public ObservableCollection<AppLimitModel> ObservableLimits
        //{
        //    get;
        //    set;
        //}

        public int ApplicationID { get; }

        public string Name { get; }

        public string FileName { get; }

        public string Version { get; }

        public string Description { get; }

        public string Company { get; }

        public string WinName { get; }

        //public UzerModel User { get; private set; }
        //public ICollection<WindowModel> Windows { get; private set; }
        //public ICollection<AppCategoryModel> Categories { get; private set; }
        //public ICollection<AppLimitModel> Limits { get; private set; }

        //public override int GetHashCode()
        //{
        //    return ApplicationID.GetHashCode();
        //}

        //public override bool Equals(object obj)
        //{
        //    var other = obj as Aplication;
        //    if (other == null)
        //        return false;

        //    return ApplicationID == other.ApplicationID;
        //}
    }
}
