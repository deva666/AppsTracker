using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using AppsTracker.MVVM;
using AppsTracker.Pages.ViewModels;

namespace AppsTracker.ViewModels
{
    internal sealed class StatisticsHostViewModel : HostViewModel, IChildVM
    {
        public string Title { get { return "statistics"; } }

        public bool IsContentLoaded { get; private set; }

        public StatisticsHostViewModel()
        {
            Register<Statistics_usersViewModel>(() => new Statistics_usersViewModel());
            Register<Statistics_appUsageViewModel>(() => new Statistics_appUsageViewModel());
            Register<Statistics_dailyAppUsageViewModel>(() => new Statistics_dailyAppUsageViewModel());
            Register<Statistics_keystrokesViewModel>(() => new Statistics_keystrokesViewModel());
            Register<Statistics_screenshotsViewModel>(() => new Statistics_screenshotsViewModel());
        }

        public void LoadContent()
        {
            SelectedChild = Resolve(typeof(Statistics_usersViewModel));
            IsContentLoaded = true;
        }

        protected override void Disposing()
        {
            _selectedChild = null;
            base.Disposing();
        }

        //protected override void ChangePage(object parameter)
        //{
        //    string viewName = parameter as string;
        //    if (viewName == null)
        //        return;
        //    switch (viewName)
        //    {
        //        case "USERS":
        //            SelectedChild = new Statistics_usersViewModel();
        //            break;
        //        case "APPS":
        //            SelectedChild = new Statistics_appUsageViewModel();
        //            break;
        //        case "DAILY APP USAGE":
        //            SelectedChild = new Statistics_dailyAppUsageViewModel();
        //            break;
        //        case "KEYSTROKES":
        //            SelectedChild = new Statistics_keystrokesViewModel();
        //            break;
        //        case "SCREENSHOTS":
        //            SelectedChild = new Statistics_screenshotsViewModel();
        //            break;
        //        default:
        //            break;
        //    }
        //}
    }
}
