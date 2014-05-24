using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Task_Logger_Pro.Models;
using Task_Logger_Pro.MVVM;

namespace Task_Logger_Pro.ViewModels
{
    class SettingsHostViewModel : HostViewModel, IChildVM
    {
        public string Title
        {
            get { return "settings"; }
        }

        public bool IsContentLoaded
        {
            get;
            private set;
        }

        public void LoadContent()
        {
            this.SelectedChild = new Settings_generalViewModel();
            //Settings_generalViewModel generalViewModel = new Settings_generalViewModel();
            //Settings_licenseViewModel licenseViewModel = new Settings_licenseViewModel();

            //var tempChildren = new List<IChildVM>(){ generalViewModel, licenseViewModel };

            //this.Children = new ReadOnlyCollection<IChildVM>(tempChildren);

            this.IsContentLoaded = true;
        }

        protected override void ChangePage(object parameter)
        {
            string viewName = parameter as string;
            if (viewName == null)
                return;

            switch (viewName)
            {
                case "APP SETTINGS":
                    SelectedChild = new Settings_generalViewModel();
                    break;
                case "LICENCE":
                    SelectedChild = new Settings_licenseViewModel();
                    break;
                default:
                    break;
            }

        }

    }
}
