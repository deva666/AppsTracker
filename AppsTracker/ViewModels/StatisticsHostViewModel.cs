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
    class StatisticsHostViewModel : HostViewModel, IChildVM
    {
        public string Title { get { return "statistics"; } }

        public bool IsContentLoaded { get; private set; }

        public void LoadContent()
        {
            this.SelectedChild = new Statistics_usersViewModel();
            this.IsContentLoaded = true;
        }

        protected override void Disposing()
        {
            this._selectedChild = null;            
            base.Disposing();
        }

        protected override void ChangePage(object parameter)
        {
            string viewName = parameter as string;
            if (viewName == null)
                return;
            switch (viewName)
            {
                case "USERS":
                    SelectedChild = new Statistics_usersViewModel();
                    break;
                case "APPS":
                    SelectedChild = new Statistics_appUsageViewModel();
                    break;
                case "DAILY APP USAGE":
                    SelectedChild = new Statistics_dailyAppUsageViewModel();
                    break;
                case "KEYSTROKES":
                    SelectedChild = new Statistics_keystrokesViewModel();
                    break;
                case "SCREENSHOTS":
                    SelectedChild = new Statistics_screenshotsViewModel();
                    break;
                default:
                    break;
            }
        }
    }
}
