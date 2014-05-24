using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Task_Logger_Pro.MVVM;
using Task_Logger_Pro.Pages.ViewModels;

namespace Task_Logger_Pro.ViewModels
{
    class StatisticsHostViewModel : HostViewModel, IChildVM
    {

        public string Title { get { return "statistics"; } }

        public bool IsContentLoaded { get; private set; }

        public void LoadContent()
        {
            this.SelectedChild = new Statistics_usersViewModel();
            //Statistics_usersViewModel usersViewModel = new Statistics_usersViewModel();
            //Statistics_screenshotsViewModel screenshotsViewModel = new Statistics_screenshotsViewModel();
            //Statistics_keystrokesViewModel keystrokesViewModel = new Statistics_keystrokesViewModel();
            //Statistics_dailyAppUsageViewModel dailyAppUsageViewModel = new Statistics_dailyAppUsageViewModel();
            //Statistics_appUsageViewModel appUsageViewModel = new Statistics_appUsageViewModel();

            //var tempChildren = new List<IChildVM>() { usersViewModel, screenshotsViewModel, keystrokesViewModel, dailyAppUsageViewModel, appUsageViewModel };

            //this.Children = new ReadOnlyCollection<IChildVM>(tempChildren);

            this.IsContentLoaded = true;
        }

        protected override void Disposing()
        {
            //if (this.Children != null)
            //{
            //    foreach (var child in this.Children)
            //    {
            //        ((ViewModelBase)child).Dispose();
            //    }
            //}
            this._selectedChild = null;
            //  this.Children = null;
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
            //if (viewName == "APPS")
            //    SelectedChild = new Data_logsViewModel();
            //else if (viewName == "DAY VIEW")
            //    SelectedChild = new Data_dayViewModel();
        }
    }
}
